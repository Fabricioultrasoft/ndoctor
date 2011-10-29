﻿/*
    This file is part of NDoctor.

    NDoctor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    NDoctor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with NDoctor.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace Probel.NDoctor.Plugins.UserSession.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Input;

    using Probel.Helpers.Conversions;
    using Probel.NDoctor.Domain.DTO.Components;
    using Probel.NDoctor.Domain.DTO.Objects;
    using Probel.NDoctor.Plugins.UserSession.Helpers;
    using Probel.NDoctor.Plugins.UserSession.Properties;
    using Probel.NDoctor.Plugins.UserSession.View;
    using Probel.NDoctor.View.Core.ViewModel;
    using Probel.NDoctor.View.Plugins.Helpers;

    using StructureMap;

    public class ConnectionViewModel : BaseViewModel
    {
        #region Fields

        private IUserSessionComponent component = ObjectFactory.GetInstance<IUserSessionComponent>();
        private string password;
        private LightUserDto selectedUser;

        #endregion Fields

        #region Constructors

        public ConnectionViewModel()
            : base()
        {
            this.Users = new ObservableCollection<LightUserDto>();

            this.ConnectCommand = new RelayCommand(() => this.Connect());
            this.NivigateAddUserCommand = new RelayCommand(() => this.NavigateToAddUer());

            Notifyer.UserAdded += (sender, e) => this.Refresh();
            this.Refresh();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the connect command.
        /// </summary>
        public ICommand ConnectCommand
        {
            get;
            private set;
        }

        public ICommand NivigateAddUserCommand
        {
            get;
            private set;
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                this.OnPropertyChanged("Password");
            }
        }

        public LightUserDto SelectedUser
        {
            get { return this.selectedUser; }
            set
            {
                this.selectedUser = value;
                this.OnPropertyChanged("SelectedUser");
            }
        }

        public ObservableCollection<LightUserDto> Users
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public void Refresh()
        {
            using (component.UnitOfWork)
            {
                var users = this.component.GetAllUsers();
                this.Users.Refill(users);
            }
        }

        private void Connect()
        {
            bool isConnected = false;
            using (this.component.UnitOfWork)
            {
                isConnected = this.component.CanConnect(this.SelectedUser, this.Password);
            }

            if (isConnected)
            {
                this.Host.ShowMainMenu();
                this.Host.ConnectedUser = this.SelectedUser;
                this.Host.NavigateToStartPage();
                this.Logger.Info("User logged in.");
            }
            else
            {
                MessageBox.Show(Messages.Msg_ErrorWrongPassword
                    , Messages.Title_Warning
                    , MessageBoxButton.OK
                    , MessageBoxImage.Asterisk);
            }
        }

        private void NavigateToAddUer()
        {
            var page = new AddUserView();
            this.Host.Navigate(page);
        }

        #endregion Methods
    }
}