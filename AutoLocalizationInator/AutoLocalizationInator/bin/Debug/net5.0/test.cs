using DevExpress.Mvvm.Native;
using EstiCon.AppServer;
using EstiCon.Client.Common;
using EstiCon.Client.Common.DataTypes;
using EstiCon.Client.Common.Enums;
using EstiCon.Client.Common.Helpers;
using EstiCon.Client.Common.MVVM.BaseClasses;
using EstiCon.Client.Common.MVVM.Models;
using EstiCon.Client.Common.MVVM.Models.Common;
using EstiCon.Client.Common.MVVM.Services;
using EstiCon.Client.Dialogs.Models;
using EstiCon.DTO;
using EstiCon.DTO.Ciselniky;
using EstiCon.DTO.Common;
using EstiCon.DTO.Enums;
using EstiCon.DTO.Master;
using EstiCon.DTO.Results;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using static EstiCon.Client.Common.Helpers.Behaviors.CopyToClipboardBehavior;

namespace EstiCon.Client.Dialogs.ViewModels
{
    public partial class AdminSettingsDialogViewModel
    {
        public DelegateCommand RoleCreateCommand { get; set; }
        public DelegateCommand RoleEditCommand { get; set; }
        public DelegateCommand RoleDeleteCommand { get; set; }
        public DelegateCommand RoleRefreshCommand { get; set; }
        public DelegateCommand RoleShowSearchBoxCommand { get; set; }
        public DelegateCommand RoleRowDoubleClickCommand { get; set; }
        public DelegateCommand RoleCopyCommand { get; set; }
        public DelegateCommand RolePasteCommand { get; set; }

        private CommonModelList<RoleDto> roleData;
        public CommonModelList<RoleDto> RoleData
        {
            get => roleData;
            set => SetProperty(ref roleData, value);
        }

        public MultiRowSelector<CommonModel<RoleDto>> RoleRowsSelector { get; }
            = new MultiRowSelector<CommonModel<RoleDto>>();

        private CommonModel<RoleDto> roleSelectedItem;

        public CommonModel<RoleDto> RoleSelectedItem
        {
            get => roleSelectedItem;
            set
            {
                if (SetProperty(ref roleSelectedItem, value))
                    RoleEditCommand.RaiseCanExecuteChanged();
            }
        }

        public Signal RoleToggleSearchPanelVisible { get; } = new Signal();


        public List<FirmDetailDto> RoleFirms { get; set; }

        private FirmDetailDto roleSelectedFirm;
        public FirmDetailDto RoleSelectedFirm
        {
            get => roleSelectedFirm;
            set
            {
                if (SetProperty(ref roleSelectedFirm, value))
                    roleLoadData();
            }
        }

        private void roleInitializeCommands()
        {
            RoleCreateCommand = new DelegateCommand(roleCreate);
            RoleEditCommand = new DelegateCommand(roleEdit, roleEditCanExecute);
            RoleDeleteCommand = new DelegateCommand(roleDeleteExecute, roleCanDelete);
            RoleRefreshCommand = new DelegateCommand(roleRefreshExecute);
            RoleShowSearchBoxCommand = new DelegateCommand(roleShowSearchBoxExecute);
            RoleRowDoubleClickCommand = new DelegateCommand(roleRowDoubleClick, roleEditCanExecute); //??
            RoleCopyCommand = new DelegateCommand(roleCopy, roleEditCanExecute);
            RolePasteCommand = new DelegateCommand(rolePaste, roleEditCanExecute);
        }

        private void roleCreate()
        {
            var result = RoleDialogViewModel.CreateRole(RoleSelectedFirm.Id);
            if (result.PressedButton == DialogResponseButton.Ok)
            {
                roleLoadData((result.Data as RoleDto)?.Id);
            }
        }

        private void roleEdit()
        {
            var result = RoleDialogViewModel.EditRole(RoleSelectedItem.Id, RoleSelectedFirm.Id);
            if (result.PressedButton == DialogResponseButton.Ok)
            {
                roleLoadData((result.Data as RoleDto)?.Id);
            }
        }

        private bool roleEditCanExecute()
        {
            return RoleSelectedItem != null;
        }

        private bool roleCanDelete()
        {
            return RoleRowsSelector.Items.Count != 0 && RoleRowsSelector.Items.All(r => r.DataDto.Count == 0);
        }

        private void roleRowDoubleClick()
        {
            if (RoleSelectedItem != null)
                RoleEditCommand.Execute();
        }

        private void roleShowSearchBoxExecute()
        {
            RoleToggleSearchPanelVisible.Fire();
        }

        private void roleRefreshExecute()
        {
            roleLoadData(RoleSelectedItem?.Id);
        }

        private void roleDeleteExecute()
        {
            var result = roleConfirmDelete(RoleRowsSelector.Items);
            if (result != DialogResponseButton.Yes)
                return;

            AppServerServiceProxy<IMasterAppService>.Instance.Use(serviceProxy =>
            {
                var toDel = new List<DtoDataBase>();
                foreach (var item in RoleRowsSelector.Items)
                {
                    if (!(item.DataDto is RoleDto dtoToDelete))
                        throw new InvalidOperationException();

                    dtoToDelete.State = ObjectState.Deleted;
                    toDel.Add(dtoToDelete);
                }

                var saveResult = serviceProxy.SaveList(toDel, new SaveParams { TargetDbId = RoleSelectedFirm.Id });
                if (saveResult.Result == SaveResult.Success)
                {
                    roleLoadData();
                    return;
                }

                errorMessage(saveResult);
            });

            if (RoleData.Count == 0)
            {
                RoleSelectedItem = null;
                RoleDeleteCommand.RaiseCanExecuteChanged();
            }
        }
        private void roleCopy()
        {
            List<RoleDto> RoleItems = new List<RoleDto>();

            if(RoleRowsSelector.Items.Count <= 0)
                return;

            AppServerServiceProxy<IModulCiselnikyAppService>.Instance.Use(serviceProxy =>
            {
                RoleRowsSelector.Items.ForEach(role =>
                {
                    RoleItems.Add(serviceProxy.RoleGetById(role.DataDto.Id, RoleSelectedFirm.Id));
                });
            });

            object serializedData = null;

            serializedData = new SerializableDataWrapper(DtoCopyFixer.GetCopyDtoData(RoleItems));
            Clipboard.SetDataObject(serializedData);
        }

        private void rolePaste()
        {
            var dataObject = Clipboard.GetDataObject();

            if (dataObject == null)
                return;

            object data = null;

            if (dataObject.GetDataPresent(DataFormats.Serializable))
                data = dataObject.GetData(DataFormats.Serializable);

            if (!(data is SerializableDataWrapper wrapper))
                return;

            var roles = wrapper.GetDtoData<RoleDto>();

            roles.ForEach(role =>
            {
                DtoCopyFixer.FixIds(role);
            });
           
            SaveResultDto result = null;
            AppServerServiceProxy<IMasterAppService>.Instance.Use(serviceProxy =>
            {
                result = serviceProxy.SaveList(
                    new List<DtoDataBase>(roles), 
                    new SaveParams { TargetDbId = RoleSelectedFirm.Id }
                    );
            });

            if (result == null)
                throw new ArgumentNullException();

            if (result.Result == SaveResult.Success)
            {
                roleLoadData();
                return;
            }

            errorMessage(result);
        }

        private DialogResponseButton roleConfirmDelete(IList<CommonModel<RoleDto>> items)
        {
            var sbMessage = new StringBuilder();
            sbMessage.AppendLine("Opravdu si přejete smazat tyto role?");
            sbMessage.AppendLine("Role:");
            foreach (var item in items)
            {
                sbMessage.AppendLine(item.DataDto.Name);
            }
            return PrismShared.DialogService.Confirm("Mazání rolí", sbMessage.ToString());
        }


        private void firmyLoadData()
        {
            AppServerServiceProxy<IMasterAppService>.Instance.Use(serviceProxy =>
            {
                RoleFirms = new List<FirmDetailDto>(serviceProxy.AllActiveFirmsGet().Items);
            });

            if (RoleFirms != null && RoleFirms.Count > 0)
                RoleSelectedFirm = RoleFirms[0];
            else
                RoleSelectedFirm = null;
        }

        private void roleLoadData(Guid? roleId = null)
        {
            if (RoleSelectedFirm == null)
            {
                RoleData = ModelFactory.CreateListFromDtoList<CommonModelList<RoleDto>, RoleDto>(new List<RoleDto>());
            }
            else
            {
                AppServerServiceProxy<IModulCiselnikyAppService>.Instance.Use(serviceProxy =>
                {
                    RoleData = ModelFactory.CreateListFromDtoList<CommonModelList<RoleDto>, RoleDto>(serviceProxy.RoleGet(RoleSelectedFirm.Id).Items);
                });
            }

            if (RoleData != null && RoleData.Count > 0)
            {
                var role = roleId.HasValue ? RoleData.SingleOrDefault(r => r.Id == roleId.Value) : RoleData[0];
                RoleSelectedItem = role ?? RoleData[0];
            }
            else
                RoleSelectedItem = null;
        }

        private void roleInitializeDialog()
        {
            firmyLoadData();

            RoleRowsSelector.SelectionChanged += (sender, args) =>
            {
                RoleDeleteCommand.RaiseCanExecuteChanged();
            };
        }

    }
}
