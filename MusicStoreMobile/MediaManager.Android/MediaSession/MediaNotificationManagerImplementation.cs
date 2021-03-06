using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Support.V4.Media.Session;
using Plugin.MediaManager.Abstractions;
using Plugin.MediaManager.Abstractions.Enums;
using System;
using NotificationCompat = Android.Support.V7.App.NotificationCompat;

namespace Plugin.MediaManager
{
    internal class MediaNotificationManagerImplementation : IAndroidMediaNotificationManager
    {
        // private MediaSessionManagerImplementation _sessionHandler;
        public IMediaQueue MediaQueue { get; set; }
        public MediaSessionCompat.Token SessionToken { get; set; }
        internal const int _notificationId = 1;

        private Intent _intent;

        private PendingIntent _pendingCancelIntent;
        private PendingIntent _pendingIntent;
        private NotificationCompat.MediaStyle _notificationStyle = new NotificationCompat.MediaStyle();
        private Context _applicationContext;
        private NotificationCompat.Builder _builder;

        public MediaNotificationManagerImplementation(Context applicationContext, Type serviceType)
        {
            _applicationContext = applicationContext;
            _intent = new Intent(_applicationContext, serviceType);
            var mainActivity =
                _applicationContext.PackageManager.GetLaunchIntentForPackage(_applicationContext.PackageName);
            _pendingIntent = PendingIntent.GetActivity(_applicationContext, 0, mainActivity,
                PendingIntentFlags.UpdateCurrent);
        }

        /// <summary>
        /// Starts the notification.
        /// </summary>
        /// <param name="mediaFile">The media file.</param>
        public void StartNotification(IMediaFile mediaFile)
        {
            StartNotification(mediaFile, true, false);
        }

        /// <summary>
        /// When we start on the foreground we will present a notification to the user
        /// When they press the notification it will take them to the main page so they can control the music
        /// </summary>
        public void StartNotification(IMediaFile mediaFile, bool mediaIsPlaying, bool canBeRemoved)
        {
            var icon = (_applicationContext.Resources?.GetIdentifier("xam_mediamanager_notify_ic", "drawable", _applicationContext?.PackageName)).GetValueOrDefault(0);

            _notificationStyle.SetMediaSession(SessionToken);
            _notificationStyle.SetCancelButtonIntent(_pendingCancelIntent);

            _builder = new NotificationCompat.Builder(_applicationContext)
            {
                MStyle = _notificationStyle
            };
            _builder.SetSmallIcon(icon != 0 ? icon : _applicationContext.ApplicationInfo.Icon);
            _builder.SetContentIntent(_pendingIntent);
            _builder.SetOngoing(mediaIsPlaying);
            _builder.SetVisibility(1);

            SetMetadata(mediaFile);
            AddActionButtons(mediaIsPlaying);

            ((NotificationCompat.MediaStyle)(_builder.MStyle)).SetShowActionsInCompactView(0, 1, 2);

            //if (_builder.MActions.Count >= 3)
            //    ((NotificationCompat.MediaStyle)(_builder.MStyle)).SetShowActionsInCompactView(0, 1, 2);
            //if (_builder.MActions.Count == 2)
            //    ((NotificationCompat.MediaStyle)(_builder.MStyle)).SetShowActionsInCompactView(0, 1);
            //if (_builder.MActions.Count == 1)
            //    ((NotificationCompat.MediaStyle)(_builder.MStyle)).SetShowActionsInCompactView(0);

            var notification = _builder.Build();
            CheckSmallIconInMIUI(notification);

            NotificationManagerCompat.From(_applicationContext).Notify(_notificationId, notification);
        }

        public void StopNotifications()
        {
            NotificationManagerCompat nm = NotificationManagerCompat.From(_applicationContext);
            nm.Cancel(_notificationId);
        }

        public void UpdateNotifications(IMediaFile mediaFile, MediaPlayerStatus status)
        {
            try
            {
                var isPlaying = status == MediaPlayerStatus.Playing || status == MediaPlayerStatus.Buffering;
                var isPersistent = status == MediaPlayerStatus.Playing || status == MediaPlayerStatus.Buffering || status == MediaPlayerStatus.Paused;
                var nm = NotificationManagerCompat.From(_applicationContext);
                if (nm != null && _builder != null)
                {
                    SetMetadata(mediaFile);
                    AddActionButtons(isPlaying);
                    _builder.SetOngoing(isPersistent);

                    var notification = _builder.Build();
                    CheckSmallIconInMIUI(notification);

                    nm.Notify(_notificationId, notification);
                }
                else
                {
                    StartNotification(mediaFile, isPlaying, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                StopNotifications();
            }
        }

        private void SetMetadata(IMediaFile mediaFile)
        {
            var title = (mediaFile?.Metadata?.Title ?? string.Empty).Trim();
            var artist = (mediaFile?.Metadata?.Artist ?? string.Empty).Trim();

            var resultTitle = ((!string.IsNullOrWhiteSpace(title)) ? (title) : ((mediaFile == null) ? ("unknown title") : ("")));
            var resultText = ((!string.IsNullOrWhiteSpace(artist)) ? (artist) : ((mediaFile == null) ? ("unknown artist") : ("")));

            _builder
                .SetContentTitle(resultTitle)
                .SetContentText(resultText)
                .SetLargeIcon(mediaFile?.Metadata?.Art as Bitmap)
                .SetShowWhen(false);

            //_builder.SetContentTitle(mediaFile?.Metadata?.Title ?? string.Empty);
            //_builder.SetContentText(mediaFile?.Metadata?.Artist ?? string.Empty);
            //_builder.SetContentInfo(mediaFile?.Metadata?.Album ?? string.Empty);
            //_builder.SetLargeIcon(mediaFile?.Metadata?.Art as Bitmap);
        }

        private Android.Support.V4.App.NotificationCompat.Action GenerateActionCompat(int icon, string title, string intentAction)
        {
            _intent.SetAction(intentAction);

            PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
            if (intentAction.Equals(MediaServiceBase.ActionStop))
                flags = PendingIntentFlags.CancelCurrent;

            PendingIntent pendingIntent = PendingIntent.GetService(_applicationContext, 1, _intent, flags);

            return new Android.Support.V4.App.NotificationCompat.Action.Builder(icon, title, pendingIntent).Build();
        }

        private void AddActionButtons(bool mediaIsPlaying)
        {
            _builder.MActions.Clear();
            _builder
                .AddAction(GenerateActionCompat(Resource.Drawable.ic_skip_previous, "Previous", MediaServiceBase.ActionPrevious))
                .AddAction(mediaIsPlaying
                    ? GenerateActionCompat(Resource.Drawable.ic_pause, "Pause", MediaServiceBase.ActionPause)
                    : GenerateActionCompat(Resource.Drawable.ic_play_arrow, "Play", MediaServiceBase.ActionPlay))
                .AddAction(GenerateActionCompat(Resource.Drawable.ic_skip_next, "Next", MediaServiceBase.ActionNext));

            //// Add previous/next button based on media queue
            //var canGoPrevious = MediaQueue?.HasPrevious() ?? false;
            //var canGoNext = MediaQueue?.HasNext() ?? false;

            //if (canGoPrevious)
            //{
            //    _builder.AddAction(GenerateActionCompat(Resource.Drawable.IcMediaPrevious, "Previous",
            //        MediaServiceBase.ActionPrevious));
            //}
            //_builder.AddAction(mediaIsPlaying
            //    ? GenerateActionCompat(Resource.Drawable.IcMediaPause, "Pause", MediaServiceBase.ActionPause)
            //    : GenerateActionCompat(Resource.Drawable.IcMediaPlay, "Play", MediaServiceBase.ActionPlay));
            //if (canGoNext)
            //{
            //    _builder.AddAction(GenerateActionCompat(Resource.Drawable.IcMediaNext, "Next",
            //        MediaServiceBase.ActionNext));
            //}
        }

        private bool CheckSmallIconInMIUI(Notification notification)
        {
            try
            {
                Java.Lang.Class miuiNotificationClass = Java.Lang.Class.ForName("android.app.MiuiNotification");
                Java.Lang.Object miuiNotification = miuiNotificationClass.NewInstance();
                Java.Lang.Reflect.Field field = miuiNotification.Class.GetDeclaredField("customizedIcon");
                field.Accessible = true;
                field.Set(miuiNotification, true);
                field = notification.Class.GetField("extraNotification");
                field.Accessible = true;
                field.Set(notification, miuiNotification);

                return true;
            }
            catch (Java.Lang.Exception e)
            {
                return false;
            }
        }
    }
}