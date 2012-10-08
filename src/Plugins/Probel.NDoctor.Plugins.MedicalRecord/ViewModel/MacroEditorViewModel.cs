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
namespace Probel.NDoctor.Plugins.MedicalRecord.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;

    using ICSharpCode.AvalonEdit.Document;

    using Probel.Mvvm.DataBinding;
    using Probel.NDoctor.Domain.DTO;
    using Probel.NDoctor.Domain.DTO.Components;
    using Probel.NDoctor.Domain.DTO.Objects;
    using Probel.NDoctor.Plugins.MedicalRecord.Helpers;
    using Probel.NDoctor.Plugins.MedicalRecord.Properties;
    using Probel.NDoctor.View.Core.Helpers;
    using Probel.NDoctor.View.Core.ViewModel;
    using Probel.NDoctor.View.Plugins.Helpers;

    public class MacroEditorViewModel : BaseViewModel
    {
        #region Fields

        private readonly IMedicalRecordComponent Component = PluginContext.ComponentFactory.GetInstance<IMedicalRecordComponent>();
        private readonly ICommand createCommand;
        private readonly ICommand refreshCommand;
        private readonly ICommand removeCommand;

        private MacroDto selectedMacro;
        private TextDocument textDocument;

        #endregion Fields

        #region Constructors

        public MacroEditorViewModel()
        {
            this.Macros = new ObservableCollection<MacroDto>();

            this.refreshCommand = new RelayCommand(() => this.Refresh(), () => this.CanRefresh());
            this.createCommand = new RelayCommand(() => this.Create(), () => this.CanCreate());
            this.removeCommand = new RelayCommand(() => this.Remove(), () => this.CanRemove());

            InnerWindow.Closed += (sender, e) => this.Save();
        }

        #endregion Constructors

        #region Properties

        public ICommand CreateCommand
        {
            get { return this.createCommand; }
        }

        public ObservableCollection<MacroDto> Macros
        {
            get;
            private set;
        }

        public ICommand RefreshCommand
        {
            get { return this.refreshCommand; }
        }

        public ICommand RemoveCommand
        {
            get { return this.removeCommand; }
        }

        public MacroDto SelectedMacro
        {
            get { return this.selectedMacro; }
            set
            {
                this.selectedMacro = value;

                var text = (value != null)
                    ? value.Expression ?? string.Empty
                    : string.Empty;

                this.TextDocument = null;
                this.TextDocument = new TextDocument(text);
                this.TextDocument.TextChanged += (sender, e) =>
                {
                    if (this.SelectedMacro != null) { this.SelectedMacro.Expression = this.TextDocument.Text; }
                };

                this.OnPropertyChanged(() => SelectedMacro);
            }
        }

        public TextDocument TextDocument
        {
            get { return this.textDocument; }
            set
            {
                this.textDocument = value;
                this.OnPropertyChanged(() => TextDocument);
            }
        }

        #endregion Properties

        #region Methods

        private bool CanCreate()
        {
            return PluginContext.DoorKeeper.IsUserGranted(To.Write);
        }

        private bool CanRefresh()
        {
            return true;
        }

        private bool CanRemove()
        {
            return true;
        }

        private void Create()
        {
            try
            {
                var macro = new MacroDto() { Title = Messages.Macro_New };
                this.Component.Create(macro);
                this.Macros.Add(macro);
            }
            catch (Exception ex) { this.HandleError(ex); }
        }

        private void Refresh()
        {
            try
            {
                var macros = this.Component.GetAllMacros();
                if (macros != null) { this.Macros.Refill(macros); }
            }
            catch (Exception ex) { this.HandleError(ex); }
        }

        private void Remove()
        {
            try
            {
                if (this.SelectedMacro != null)
                {
                    this.Component.Remove(this.SelectedMacro);
                    this.Macros.Remove(this.SelectedMacro);
                    this.refreshCommand.TryExecute();
                }
            }
            catch (Exception ex) { this.HandleError(ex); }
        }

        private void Save()
        {
            try
            {
                this.Component.Update(this.Macros);

                PluginContext.Host.WriteStatus(StatusType.Info, Messages.Msg_MacrosUpdated);
                Notifyer.OnMacroUpdated();
            }
            catch (Exception ex) { this.HandleError(ex); }
        }

        #endregion Methods
    }
}