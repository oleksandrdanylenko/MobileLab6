using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;

namespace MusicStoreMobile.Core.Services.Interfaces
{
    public interface INotificationService
    {
        void ShowNotification<VM>() where VM : IMvxViewModel;
    }
}
