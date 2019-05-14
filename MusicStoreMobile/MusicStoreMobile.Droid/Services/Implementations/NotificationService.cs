using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Support.V4.App;
using MusicStoreMobile.Core.Services.Interfaces;
using MusicStoreMobile.Droid.Views;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

namespace MusicStoreMobile.Droid.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// THIS IS RELEASED
        /// </summary>
        /// <typeparam name="VM"></typeparam>

        public void ShowNotification<VM>() where VM : IMvxViewModel
        {
            var notification = new NotificationCompat.Builder(Application.Context)
                .SetContentTitle("Music Player")
                .SetContentText("You were successfully registered. Click there.")
                .SetSmallIcon(Resource.Drawable.icon)
                .SetContentIntent(GetContentIntent<VM>())
                .SetShowWhen(false)
                .SetOnlyAlertOnce(true)
                .SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 })
                .SetSound(Settings.System.DefaultNotificationUri)
                .Build();

            var notificationManager = NotificationManagerCompat.From(Application.Context);
            notificationManager.Notify(1, notification);
        }

        private PendingIntent GetContentIntent<VM>() where VM : IMvxViewModel
        {
            var request = MvxViewModelRequest<VM>.GetDefaultRequest();

            var converter = Mvx.Resolve<IMvxNavigationSerializer>();
            var requestText = converter.Serializer.SerializeObject(request);

            var intent = new Intent(Application.Context, typeof(MainView));

            // We only want one activity started
            intent.AddFlags(ActivityFlags.SingleTop);

            intent.PutExtra("MvxLaunchData", requestText);

            // Create Pending intent, with OneShot. We're not going to want to update this.
            return PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.OneShot);
        }

    }
}