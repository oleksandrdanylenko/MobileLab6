using Android.App;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MusicStoreMobile.Core.ViewModels;
using MvvmCross.Droid.Views.Attributes;
using Plugin.MediaManager;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Abstractions.Implementations;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;   

namespace MusicStoreMobile.Droid.Views
{
    [MvxFragmentPresentation(typeof(MainViewModel), Resource.Id.audio_player_frame, false)]
    [Register(nameof(AudioPlayerView))]
    public class AudioPlayerView : BaseFragment<AudioPlayerViewModel>
    {
        protected override int FragmentId => Resource.Layout.AudioPlayerView;

        private TextView _audioPlayerTitleText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            _audioPlayerTitleText = view.FindViewById<TextView>(Resource.Id.audio_player_title_text);
            if(_audioPlayerTitleText != null)
            {
                //_audioPlayerTitleText.Selected = true;
            }

            if (savedInstanceState == null)
            {
                var rootDirectory =  Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).ToString();

                ViewModel.Queue.Clear();
                var list = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).ListFiles();
                var mediaUrls =
                    new[] {
                        Path.Combine(rootDirectory, "James Arthur - Impossible (music7s.me).mp3"),
                        Path.Combine(rootDirectory, "Charlie Puth - Attention.mp3"),
                        Path.Combine(rootDirectory, "Fly Project - Dame (Radio Edit).mp3"),
                        Path.Combine(rootDirectory, "wiz_khalifa___kill_the_noise__madsonik_-_shell_shocked.mp3"),
                        Path.Combine(rootDirectory, "imagine_dragons_-_thunder_(zf.fm) — копия.mp3"),
                        Path.Combine(rootDirectory, "Imagine Dragons  - Battle Cry (music7s.me).mp3"),
                        Path.Combine(rootDirectory, "Boyce Avenue  - What Makes You Beautiful   (#ACOUSTIC) (music7s.me).mp3"),
                        Path.Combine(rootDirectory, "Calvin Harris - We'll Be Coming Back (ft. Example) (music7s.me).mp3"),
                        Path.Combine(rootDirectory, "Tyrone Wells - More (music7s.me).mp3"),
                        Path.Combine(rootDirectory, "Galantis - Hunter.mp3")
                    };
                //mediaUrls.AddRange(list.Select(x => x.AbsolutePath));


                foreach (var mediaUrl in mediaUrls)
                {

                    //Local перевіряє наявність файлів, якщо не існує, видаляє з черги
                    //Remote не перевіряж наявність файлів, якщо не існує, намагається відтврити, якщо дійшла її черга
                    ViewModel.Queue.Add(new MediaFile(mediaUrl, MediaFileType.Audio, ResourceAvailability.Local));
                }

                //ViewModel.Queue.Repeat = RepeatType.RepeatAll;

                ParentActivity?.RunOnUiThread(() => ViewModel.RaiseAllPropertiesChanged());
                ParentActivity?.RunOnUiThread(() => ViewModel.MediaPlayer.Play(ViewModel.CurrentTrack));
            }

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnDestroy()
        {
            _audioPlayerTitleText?.Dispose();
            base.OnDestroy();
        }
    }
}