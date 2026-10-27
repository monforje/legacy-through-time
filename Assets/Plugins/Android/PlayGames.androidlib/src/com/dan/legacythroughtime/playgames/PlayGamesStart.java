package com.dan.legacythroughtime.playgames;

import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;

import com.google.android.gms.games.PlayGamesSdk;

/**
 * Starts Play Games with the app, before the first screen, so that its own sign-in begins at once - in place of the
 * SDK's PlayGamesInitProvider (removed in the manifest), which would do it without a project id too.
 */
public final class PlayGamesStart extends ContentProvider {
    @Override
    public boolean onCreate() {
        Context context = getContext();
        if (context != null && PlayGamesBridge.configured(context)) PlayGamesSdk.initialize(context);
        return true;
    }

    @Override
    public Cursor query(Uri uri, String[] projection, String selection, String[] selectionArgs, String sortOrder) {
        return null;
    }

    @Override
    public String getType(Uri uri) {
        return null;
    }

    @Override
    public Uri insert(Uri uri, ContentValues values) {
        return null;
    }

    @Override
    public int delete(Uri uri, String selection, String[] selectionArgs) {
        return 0;
    }

    @Override
    public int update(Uri uri, ContentValues values, String selection, String[] selectionArgs) {
        return 0;
    }
}
