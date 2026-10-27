package com.dan.legacythroughtime.playgames;

import android.app.Activity;
import android.content.Context;

import com.google.android.gms.games.AuthenticationResult;
import com.google.android.gms.games.GamesSignInClient;
import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.PlayGamesSdk;
import com.google.android.gms.games.Player;
import com.google.android.gms.games.SnapshotsClient;
import com.google.android.gms.games.snapshot.Snapshot;
import com.google.android.gms.games.snapshot.SnapshotMetadataChange;
import com.google.android.gms.tasks.Task;
import com.google.android.gms.tasks.Tasks;

import java.io.IOException;
import java.nio.charset.StandardCharsets;

/**
 * Play Games Services v2 for PlayGames.cs: the sign-in, the player, and saved games read and written as text. Every
 * call answers exactly once through its Callback, on the Android main thread (C# moves the answer to Unity's).
 */
public final class PlayGamesBridge {
    public interface Callback {
        void done(boolean ok, String data);
    }

    // two phones that saved offline both reach the server: the newer copy is kept (the game merges the phone's own
    // progress into it on its next sync, so nothing of the older one is lost for good)
    private static final int CONFLICT_ROUNDS = 10;

    private PlayGamesBridge() {}

    /** False until res/values/play_games.xml holds the project id: without one Play Games is never started or called. */
    public static boolean configured(Context context) {
        String id = context.getString(R.string.game_services_project_id).trim();
        return id.matches("[0-9]+") && !id.matches("0+");
    }

    /**
     * Not `interactive`: the sign-in Play Games makes by itself when the game starts (no screen of the game's own).
     * `interactive`: the Play Games sign-in screen, after a tap of the reader. Answers "id\nname" of the player.
     */
    public static void signIn(Activity activity, boolean interactive, Callback callback) {
        try {
            PlayGamesSdk.initialize(activity.getApplicationContext());      // done by PlayGamesStart already; a no-op then
            GamesSignInClient client = PlayGames.getGamesSignInClient(activity);
            Task<AuthenticationResult> auth = interactive ? client.signIn() : client.isAuthenticated();
            auth.addOnCompleteListener(t -> {
                if (!t.isSuccessful() || !t.getResult().isAuthenticated()) {
                    callback.done(false, t.isSuccessful() ? "not signed in" : message(t.getException()));
                    return;
                }
                PlayGames.getPlayersClient(activity).getCurrentPlayer().addOnCompleteListener(p -> {
                    if (!p.isSuccessful()) {
                        callback.done(false, message(p.getException()));
                        return;
                    }
                    Player player = p.getResult();
                    callback.done(true, player.getPlayerId() + "\n" + player.getDisplayName());
                });
            });
        } catch (RuntimeException e) {
            callback.done(false, message(e));
        }
    }

    /** The text of the saved game `name`: "" when it was never written. */
    public static void load(Activity activity, String name, Callback callback) {
        try {
            SnapshotsClient client = PlayGames.getSnapshotsClient(activity);
            open(client, name).addOnCompleteListener(t -> {
                if (!t.isSuccessful()) {
                    callback.done(false, message(t.getException()));
                    return;
                }
                Snapshot snapshot = t.getResult();
                try {
                    byte[] bytes = snapshot.getSnapshotContents().readFully();
                    client.discardAndClose(snapshot);
                    callback.done(true, new String(bytes, StandardCharsets.UTF_8));
                } catch (IOException | RuntimeException e) {
                    client.discardAndClose(snapshot);
                    callback.done(false, message(e));
                }
            });
        } catch (RuntimeException e) {
            callback.done(false, message(e));
        }
    }

    /** Replaces the saved game `name` with `data`; `description` is what Play Games shows for it. Offline, Play Games
     *  keeps it and sends it when the network is back. */
    public static void save(Activity activity, String name, String data, String description, Callback callback) {
        try {
            SnapshotsClient client = PlayGames.getSnapshotsClient(activity);
            open(client, name).addOnCompleteListener(t -> {
                if (!t.isSuccessful()) {
                    callback.done(false, message(t.getException()));
                    return;
                }
                Snapshot snapshot = t.getResult();
                snapshot.getSnapshotContents().writeBytes(data.getBytes(StandardCharsets.UTF_8));
                SnapshotMetadataChange change = new SnapshotMetadataChange.Builder().setDescription(description).build();
                client.commitAndClose(snapshot, change).addOnCompleteListener(c ->
                        callback.done(c.isSuccessful(), c.isSuccessful() ? "" : message(c.getException())));
            });
        } catch (RuntimeException e) {
            callback.done(false, message(e));
        }
    }

    private static Task<Snapshot> open(SnapshotsClient client, String name) {
        return client.open(name, true, SnapshotsClient.RESOLUTION_POLICY_MOST_RECENTLY_MODIFIED)
                .continueWithTask(t -> settle(client, t.getResult(), 0));
    }

    private static Task<Snapshot> settle(SnapshotsClient client, SnapshotsClient.DataOrConflict<Snapshot> result, int round) {
        if (!result.isConflict()) return Tasks.forResult(result.getData());
        if (round >= CONFLICT_ROUNDS) return Tasks.forException(new IllegalStateException("the saved game keeps conflicting"));
        SnapshotsClient.SnapshotConflict conflict = result.getConflict();
        Snapshot server = conflict.getSnapshot(), local = conflict.getConflictingSnapshot();
        Snapshot newer = server.getMetadata().getLastModifiedTimestamp() >= local.getMetadata().getLastModifiedTimestamp() ? server : local;
        return client.resolveConflict(conflict.getConflictId(), newer)
                .continueWithTask(t -> settle(client, t.getResult(), round + 1));
    }

    private static String message(Exception e) {
        return e == null ? "unknown error" : e.getClass().getSimpleName() + ": " + e.getMessage();
    }
}
