using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TelltaleTextureTool.Main;
using TelltaleTextureTool.Utilities;
using TelltaleTextureTool.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using IImage = Avalonia.Media.IImage;
using TelltaleTextureTool.DirectX;

namespace TelltaleTextureTool.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region MEMBERS

    private readonly ObservableCollection<FormatItemViewModel> _d3dtxTypes =
    [
        new FormatItemViewModel { Name = "DDS", ItemStatus = true },
        new FormatItemViewModel { Name = "PNG", ItemStatus = false },
        new FormatItemViewModel { Name = "KTX", ItemStatus = false },
        new FormatItemViewModel { Name = "KTX2", ItemStatus = false }
    ];

    private readonly ObservableCollection<FormatItemViewModel> _ddsTypes =
    [
        new FormatItemViewModel { Name = "D3DTX", ItemStatus = true},
        new FormatItemViewModel { Name = "PNG", ItemStatus = true },
        new FormatItemViewModel { Name = "JPEG", ItemStatus = true },
        new FormatItemViewModel { Name = "BMP", ItemStatus = true },
        new FormatItemViewModel { Name = "TIFF", ItemStatus = true },
        new FormatItemViewModel { Name = "TGA", ItemStatus = false }
    ];

    private readonly ObservableCollection<FormatItemViewModel> _ktxTypes =
    [
        new FormatItemViewModel { Name = "D3DTX", ItemStatus = true}
    ];

    private readonly ObservableCollection<FormatItemViewModel> _otherTypes =
        [new FormatItemViewModel { Name = "DDS", ItemStatus = true }];

    private readonly ObservableCollection<FormatItemViewModel> _folderTypes =
    [
        new FormatItemViewModel { Name = "D3DTX -> DDS", ItemStatus = true},
        new FormatItemViewModel { Name = "D3DTX -> PNG", ItemStatus = false},
        new FormatItemViewModel { Name = "D3DTX -> KTX", ItemStatus = false},
        new FormatItemViewModel { Name = "D3DTX -> KTX2", ItemStatus = false},
        new FormatItemViewModel { Name = "DDS -> D3DTX", ItemStatus = true},
        new FormatItemViewModel { Name = "DDS -> PNG", ItemStatus = true},
        new FormatItemViewModel { Name = "DDS -> JPEG", ItemStatus = true},
        new FormatItemViewModel { Name = "DDS -> BMP", ItemStatus = true},
        new FormatItemViewModel { Name = "DDS -> TIFF", ItemStatus = true},
        new FormatItemViewModel { Name = "DDS -> TGA", ItemStatus = false},
        new FormatItemViewModel { Name = "PNG -> DDS", ItemStatus = true},
        new FormatItemViewModel { Name = "JPEG -> DDS", ItemStatus = true},
        new FormatItemViewModel { Name = "BMP -> DDS", ItemStatus = true},
        new FormatItemViewModel { Name = "TIFF -> DDS", ItemStatus = true},
        new FormatItemViewModel { Name = "TGA -> DDS", ItemStatus = false}
    ];

    private readonly ObservableCollection<FormatItemViewModel> _versionConvertOptions =
    [
        new FormatItemViewModel { Name = "Default", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 1", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 2", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 3", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 4", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 5", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 6", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 7", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 8", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 9", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 10", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 11", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 12", ItemStatus = true},
        new FormatItemViewModel { Name = "Legacy Version 13", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 1", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 2", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 3", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 4", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 5", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 6", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 7", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 8", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 9", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 10", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 11", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 12", ItemStatus = true},
        new FormatItemViewModel { Name = "Console Legacy Version 13", ItemStatus = true},
    ];

    private readonly List<string> _allTypes = [".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff", ".d3dtx", ".dds", ".ktx", ".ktx2", ".tga"];

    // No idea if this is correct
    public static FilePickerFileType AllowedTypes { get; } = new("All Supported Types")
    {
        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tif", "*.tiff", "*.d3dtx", "*.dds", "*.ktx", "*.ktx2", "*.tga", "*.json"],
        AppleUniformTypeIdentifiers = ["public.image"],
        MimeTypes = ["image/png", "image/jpeg", "image/bmp", "image/tiff", "image/tga", "image/vnd.ms-dds", "image/vnd.ms-d3dtx", "image/vnd.ms-ktx", "image/vnd.ms-ktx2"]
    };

    private readonly MainManager mainManager = MainManager.GetInstance();
    private readonly Uri _assetsUri = new("avares://TelltaleTextureTool/Assets/");
    private static readonly string ErrorSvgFilename = "error.svg";

    #endregion

    #region UI PROPERTIES

    [ObservableProperty] private ImageProperties _imageProperties;
    [ObservableProperty] private FormatItemViewModel _selectedFormat;

    [ObservableProperty] private FormatItemViewModel _selectedVersionConvertOption;
    [ObservableProperty] private ObservableCollection<FormatItemViewModel> _formatsList = [];
    [ObservableProperty] private ObservableCollection<FormatItemViewModel> _versionConvertOptionsList = [];
    [ObservableProperty] private bool _comboBoxStatus;
    [ObservableProperty] private bool _versionConvertComboBoxStatus;
    [ObservableProperty] private bool _saveButtonStatus;
    [ObservableProperty] private bool _deleteButtonStatus;
    [ObservableProperty] private bool _convertButtonStatus;
    [ObservableProperty] private bool _debugButtonStatus;
    [ObservableProperty] private bool _contextOpenFolderStatus;
    [ObservableProperty] private bool _chooseOutputDirectoryCheckBoxEnabledStatus;

    [ObservableProperty] private int _selectedComboboxIndex;
    [ObservableProperty] private int _selectedLegacyTitleIndex;
    [ObservableProperty] private string? _imageNamePreview;
    [ObservableProperty] private IImage? _imagePreview;
    [ObservableProperty] private string _fileText = string.Empty;
    [ObservableProperty] private string _directoryPath = string.Empty;
    [ObservableProperty] private bool _returnDirectoryButtonStatus;
    [ObservableProperty] private bool _refreshDirectoryButtonStatus;
    [ObservableProperty] private bool _chooseOutputDirectoryCheckboxStatus;
    [ObservableProperty] private string _debugInfo = string.Empty;

    [ObservableProperty][NotifyCanExecuteChangedFor("PreviewImageCommand")] private uint _mipValue;
    [ObservableProperty][NotifyCanExecuteChangedFor("PreviewImageCommand")] private uint _faceValue;
    [ObservableProperty] private uint _maxMipCount;
    [ObservableProperty] private uint _maxFaceCount;
    [ObservableProperty] private ObservableCollection<WorkingDirectoryFile> _workingDirectoryFiles = [];

    [ObservableProperty] private ImageData _imageData;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("ResetPanAndZoomCommand")]
    private WorkingDirectoryFile _dataGridSelectedItem = new();


    public class FormatItemViewModel
    {
        public string? Name { get; set; }
        public bool ItemStatus { get; set; }
    }

    public RelayCommand ResetPanAndZoomCommand { get; internal set; }

    private void ResetPanAndZoom()
    {
        // Logic to reset pan and zoom
        // This method will be linked with code-behind to reset the ZoomBorder.
    }

    #endregion

    public MainViewModel()
    {
        ImagePreview = new SvgImage()
        {
            Source = SvgSource.Load(ErrorSvgFilename, _assetsUri)
        };
        VersionConvertOptionsList = _versionConvertOptions;
        SelectedVersionConvertOption = VersionConvertOptionsList[0];
    }

    #region MAIN MENU BUTTONS ACTIONS

    // Open Directory Command
    [RelayCommand]
    public async Task OpenDirectoryButton_Click()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            await mainManager.SetWorkingDirectoryPath(provider);

            if (mainManager.GetWorkingDirectoryPath() != string.Empty)
            {
                ReturnDirectoryButtonStatus = true;
                RefreshDirectoryButtonStatus = true;
                DataGridSelectedItem = null;

                await UpdateUiAsync();
            }
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(e.Message);
        }
    }

    [RelayCommand]
    public async Task SaveFileButton_Click()
    {
        try
        {
            if (DataGridSelectedItem is not null)
            {
                var topLevel = GetMainWindow();

                if (Directory.Exists(DataGridSelectedItem.FilePath))
                {
                    throw new Exception("Cannot save a directory.");
                }

                // Start async operation to open the dialog.
                var storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save File",
                    SuggestedFileName = DataGridSelectedItem.FileName,
                    ShowOverwritePrompt = true,
                    DefaultExtension = DataGridSelectedItem.FileType is null ? "bin" : DataGridSelectedItem.FileType.Substring(1)
                });

                if (storageFile is not null)
                {
                    var destinationFilePath = storageFile.Path.AbsolutePath;

                    if (File.Exists(DataGridSelectedItem.FilePath))
                        File.Copy(DataGridSelectedItem.FilePath, destinationFilePath, true);
                }
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync("Error during saving the file. " + ex.Message);
        }
        finally
        {
            await SafeRefreshDirectoryAsync();
            await UpdateUiAsync();
        }
    }

    [RelayCommand]
    public async Task AddFilesButton_Click()
    {
        try
        {
            if (string.IsNullOrEmpty(DirectoryPath) || !Directory.Exists(DirectoryPath)) return;

            var topLevel = GetMainWindow();

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Files",
                AllowMultiple = true,
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(DirectoryPath),
                FileTypeFilter = [AllowedTypes]
            });

            foreach (var file in files)
            {
                var destinationFilePath = Path.Combine(DirectoryPath, file.Name);

                var i = 1;
                while (File.Exists(destinationFilePath))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    var extension = Path.GetExtension(file.Name);
                    destinationFilePath = Path.Combine(DirectoryPath,
                        $"{fileNameWithoutExtension}({i++}){extension}");
                }

                File.Copy(new Uri(file.Path.ToString()).LocalPath, destinationFilePath);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync("Error during adding files. Some files were not copied. " + ex.Message);
        }

        await SafeRefreshDirectoryAsync();
        await UpdateUiAsync();
    }

    // Delete Command
    [RelayCommand]
    public async Task DeleteFileButton_Click()
    {
        var workingDirectoryFile =
            DataGridSelectedItem;

        var textureFilePath = workingDirectoryFile.FilePath;

        try
        {
            if (File.Exists(textureFilePath))
            {
                File.Delete(textureFilePath);
            }
            else if (Directory.Exists(textureFilePath))
            {
                var mainWindow = GetMainWindow();
                var messageBox =
                    MessageBoxes.GetConfirmationBox("Are you sure you want to delete this directory?");

                var result = await MessageBoxManager.GetMessageBoxStandard(messageBox)
                    .ShowWindowDialogAsync(mainWindow);

                if (result != ButtonResult.Yes) return;

                Directory.Delete(textureFilePath);
            }
            else
            {
                throw new Exception("Invalid file or directory path.");
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
        finally
        {
            DataGridSelectedItem = null;
            await SafeRefreshDirectoryAsync();
            await UpdateUiAsync();
        }
    }

    [RelayCommand]
    public void HelpButton_Click()
    {
        mainManager.OpenAppHelp();
    }


    [RelayCommand]
    public void AboutButton_Click()
    {
        var mainWindow = GetMainWindow();
        var aboutWindow = new AboutWindow
        {
            DataContext = new AboutViewModel()
        };

        aboutWindow.ShowDialog(mainWindow);
    }

    #endregion

    #region CONTEXT MENU ACTIONS

    [RelayCommand]
    public async Task ContextMenuAddFilesCommand()
    {
        try
        {
            if (string.IsNullOrEmpty(DirectoryPath) || !Directory.Exists(DirectoryPath)) return;

            var topLevel = GetMainWindow();

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Files",
                AllowMultiple = true
            });

            foreach (var file in files)
            {
                var destinationFilePath = Path.Combine(DirectoryPath, file.Name);

                var i = 1;
                while (File.Exists(destinationFilePath))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    var extension = Path.GetExtension(file.Name);
                    destinationFilePath = Path.Combine(DirectoryPath,
                        $"{fileNameWithoutExtension}({i++}){extension}");
                }

                File.Copy(file.Path.AbsolutePath, destinationFilePath);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync("Error during adding files. Some files were not copied." + ex.Message);
        }
        finally
        {

        }

        await SafeRefreshDirectoryAsync();
        await UpdateUiAsync();
    }

    [RelayCommand]
    public async Task ContextMenuOpenFileCommand()
    {
        try
        {
            if (DataGridSelectedItem == null)
                return;

            var workingDirectoryFile =
                DataGridSelectedItem;

            var filePath = workingDirectoryFile.FilePath;

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
                throw new DirectoryNotFoundException("Directory was not found");

            mainManager.OpenFile(filePath);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
    }

    [RelayCommand]
    public async Task ContextMenuOpenFolderCommand()
    {
        try
        {
            // if there is no valid item selected, don't continue
            if (DataGridSelectedItem == null)
                return;

            // get our selected file object from the working directory
            var workingDirectoryFile = DataGridSelectedItem;
            if (!Directory.Exists(workingDirectoryFile.FilePath))
                throw new DirectoryNotFoundException("Directory not found.");

            await mainManager.SetWorkingDirectoryPath(workingDirectoryFile.FilePath);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
        finally
        {
            ContextOpenFolderStatus = false;
            await UpdateUiAsync();
        }
    }

    [RelayCommand]
    public async Task ContextMenuOpenFileExplorerCommand()
    {
        try
        {
            if (DirectoryPath == null) return;

            if (DataGridSelectedItem == null)
            {
                if (Directory.Exists(DirectoryPath))
                    await OpenFileExplorer(DirectoryPath);
            }
            else
            {
                if (File.Exists(DataGridSelectedItem.FilePath))
                    await OpenFileExplorer(DataGridSelectedItem.FilePath);
                else if (Directory.Exists(DataGridSelectedItem.FilePath))
                    await OpenFileExplorer(DataGridSelectedItem.FilePath);
            }
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
    }

    [RelayCommand]
    public async Task RefreshDirectoryButton_Click()
    {
        if (DirectoryPath != null && DirectoryPath != string.Empty)
        {
            await RefreshUiAsync();
        }
    }

    [RelayCommand]
    public async Task ContextDeleteFileCommand()
    {
        await DeleteFileButton_Click();
    }

    public async Task SafeRefreshDirectoryAsync()
    {
        try
        {
            mainManager.RefreshWorkingDirectory();
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
    }

    #endregion

    #region CONVERTER PANEL ACTIONS

    /// <summary>
    /// Convert command of the "Convert to" button. It initiates the conversion process.
    /// Error dialogs appear when something goes wrong with the conversion process.
    /// </summary>
    [RelayCommand]
    public async Task ConvertButton_Click()
    {
        try
        {
            if (DataGridSelectedItem == null) return;

            var workingDirectoryFile =
                DataGridSelectedItem;

            string? textureFilePath = workingDirectoryFile.FilePath;

            if (!File.Exists(textureFilePath) && !Directory.Exists(textureFilePath))
                throw new DirectoryNotFoundException("File/Directory was not found.");

            string outputDirectoryPath = mainManager.GetWorkingDirectoryPath();

            if (ChooseOutputDirectoryCheckboxStatus)
            {
                var topLevel = GetMainWindow();

                // Start async operation to open the dialog.
                var folderPath = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Choose your output folder location.",
                    AllowMultiple = false,
                });

                if (folderPath is null || folderPath.Count == 0)
                {
                    return;
                }

                outputDirectoryPath = folderPath.First().Path.AbsolutePath;
            }

            D3DTXVersion conversionType = GetD3DTXConversionType();

            string[] types = SelectedFormat.Name.Split(" -> ");

            TextureType oldTextureType;
            TextureType newTextureType;

            if (types.Length == 2)
            {
                oldTextureType = GetTextureTypeFromExtension(types[0]);
                newTextureType = GetTextureTypeFromExtension(types[1]);

                if (!ChooseOutputDirectoryCheckboxStatus)
                {
                    outputDirectoryPath = textureFilePath;
                }

                Converter.ConvertBulk(textureFilePath, outputDirectoryPath, oldTextureType, newTextureType, conversionType);
            }
            else
            {
                oldTextureType = GetTextureTypeFromExtension(DataGridSelectedItem.FileType);
                newTextureType = GetTextureTypeFromItem(SelectedFormat.Name);
                Console.WriteLine("Old Texture Type: " + oldTextureType);
                Console.WriteLine("New Texture Type: " + newTextureType);
                await Converter.ConvertTexture(textureFilePath, outputDirectoryPath, oldTextureType, newTextureType, conversionType);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            var mainWindow = GetMainWindow();
            var messageBox =
                MessageBoxes.GetErrorBox(ex.Message);
            await MessageBoxManager.GetMessageBoxStandard(messageBox).ShowWindowDialogAsync(mainWindow);
            Logger.Instance().Log(ex);
        }
        finally
        {
            mainManager.RefreshWorkingDirectory();
            await UpdateUiAsync();
        }
    }

    private static TextureType GetTextureTypeFromItem(string newTextureType)
    {
        return newTextureType switch
        {
            "D3DTX" => TextureType.D3DTX,
            "DDS" => TextureType.DDS,
            "PNG" => TextureType.PNG,
            "JPG" => TextureType.JPEG,
            "JPEG" => TextureType.JPEG,
            "BMP" => TextureType.BMP,
            "TIF" => TextureType.TIFF,
            "TIFF" => TextureType.TIFF,
            "KTX" => TextureType.KTX,
            "KTX2" => TextureType.KTX2,
            "TGA" => TextureType.TGA,
            _ => TextureType.Unknown
        };
    }

    private D3DTXVersion GetD3DTXConversionType()
    {
        return SelectedVersionConvertOption.Name switch
        {
            "Legacy Version 1" => D3DTXVersion.LV1,
            "Legacy Version 2" => D3DTXVersion.LV2,
            "Legacy Version 3" => D3DTXVersion.LV3,
            "Legacy Version 4" => D3DTXVersion.LV4,
            "Legacy Version 5" => D3DTXVersion.LV5,
            "Legacy Version 6" => D3DTXVersion.LV6,
            "Legacy Version 7" => D3DTXVersion.LV7,
            "Legacy Version 8" => D3DTXVersion.LV8,
            "Legacy Version 9" => D3DTXVersion.LV9,
            "Legacy Version 10" => D3DTXVersion.LV10,
            "Legacy Version 11" => D3DTXVersion.LV11,
            "Legacy Version 12" => D3DTXVersion.LV12,
            "Legacy Version 13" => D3DTXVersion.LV13,
            "Console Legacy Version 1" => D3DTXVersion.CLV1,
            "Console Legacy Version 2" => D3DTXVersion.CLV2,
            "Console Legacy Version 3" => D3DTXVersion.CLV3,
            "Console Legacy Version 4" => D3DTXVersion.CLV4,
            "Console Legacy Version 5" => D3DTXVersion.CLV5,
            "Console Legacy Version 6" => D3DTXVersion.CLV6,
            "Console Legacy Version 7" => D3DTXVersion.CLV7,
            "Console Legacy Version 8" => D3DTXVersion.CLV8,
            "Console Legacy Version 9" => D3DTXVersion.CLV9,
            "Console Legacy Version 10" => D3DTXVersion.CLV10,
            "Console Legacy Version 11" => D3DTXVersion.CLV11,
            "Console Legacy Version 12" => D3DTXVersion.CLV12,
            "Console Legacy Version 13" => D3DTXVersion.CLV13,
            _ => D3DTXVersion.DEFAULT
        };
    }

    private static TextureType GetTextureTypeFromExtension(string newTextureType)
    {
        return GetTextureTypeFromItem(newTextureType.ToUpper().Remove(0, 1));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DebugButton_Click()
    {
        try
        {
            if (DataGridSelectedItem == null) return;

            var workingDirectoryFile =
                DataGridSelectedItem;

            string? textureFilePath = workingDirectoryFile.FilePath;

            string debugInfo = string.Empty;

            if (workingDirectoryFile.FileType == ".d3dtx")
            {
                D3DTXVersion conversionType = GetD3DTXConversionType();

                var d3dtx = new D3DTX_Master();
                d3dtx.ReadD3DTXFile(textureFilePath, conversionType);

                debugInfo = d3dtx.GetD3DTXDebugInfo();
            }
            else if (workingDirectoryFile.FileType == ".dds")
            {
                debugInfo = DDS_DirectXTexNet.GetDDSDebugInfo(textureFilePath);
            }
            else
            {
                debugInfo = string.Empty;
            }

            DebugInfo = debugInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            await HandleExceptionAsync(ex.Message);
        }
    }

    #endregion

    ///<summary>
    /// Updates our application UI, mainly the data grid.
    ///</summary>
    private async Task UpdateUiAsync()
    {
        // update our texture directory UI
        try
        {
            DirectoryPath = mainManager.GetWorkingDirectoryPath();
            mainManager.RefreshWorkingDirectory();
            var workingDirectoryFiles = mainManager.GetWorkingDirectoryFiles();

            for (int i = WorkingDirectoryFiles.Count - 1; i >= 0; i--)
            {
                if (!workingDirectoryFiles.Contains(WorkingDirectoryFiles[i]))
                {
                    WorkingDirectoryFiles.RemoveAt(i);
                }
            }

            // Add items from the list to the observable collection if they are not already present
            foreach (var item in workingDirectoryFiles)
            {
                if (!WorkingDirectoryFiles.Contains(item))
                {
                    WorkingDirectoryFiles.Add(item);
                }
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            await HandleExceptionAsync("Error during updating UI. " + ex.Message);
        }
    }

    #region SMALL MENU BUTTON ACTIONS

    [RelayCommand]
    public async Task ReturnDirectory_Click()
    {
        try
        {
            if (Directory.GetParent(DirectoryPath) == null) return;
            WorkingDirectoryFiles.Clear();
            await mainManager.SetWorkingDirectoryPath(Directory.GetParent(DirectoryPath).ToString());
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex.Message);
        }
        finally
        {
            await PreviewImage();
            await UpdateUiAsync();
        }
    }

    [RelayCommand]
    public async Task ContextMenuRefreshDirectoryCommand()
    {
        await RefreshDirectoryButton_Click();
    }

    #endregion

    #region HELPERS

    private Window GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            return lifetime.MainWindow;

        throw new Exception("Main Parent Window Not Found");
    }

    private void ChangeComboBoxItemsByItemExtension(string itemExtension)
    {
        var extensionMappings = new Dictionary<string, ObservableCollection<FormatItemViewModel>>
        {
            { ".dds", _ddsTypes },
            { ".d3dtx", _d3dtxTypes },
            {".ktx", _otherTypes},
            {".ktx2", _otherTypes},
            { ".png", _otherTypes },
            { ".jpg", _otherTypes },
            { ".jpeg", _otherTypes },
            { ".bmp", _otherTypes },
            { ".tga", _otherTypes },
            { ".tif", _otherTypes },
            { ".tiff", _otherTypes },
            {"", _folderTypes}
        };

        if (itemExtension == null)
        {
            FormatsList = null;
            ConvertButtonStatus = false;
            ComboBoxStatus = false;
        }
        else if (extensionMappings.TryGetValue(itemExtension, out var selectedItems))
        {
            if (itemExtension.Equals(".d3dtx"))
                VersionConvertComboBoxStatus = true;
            else
                VersionConvertComboBoxStatus = false;

            FormatsList = selectedItems;
            ConvertButtonStatus = true;
            SelectedComboboxIndex = 0;
            ComboBoxStatus = true;
            // There is an issue in Avalonia relating to dynamic sources and binding indexes.
            // Github issue: https://github.com/AvaloniaUI/Avalonia/issues/13736
            // When fixed, the line below can be removed.
            SelectedFormat = selectedItems[0];
        }
        else
        {
            FormatsList = null;
            ConvertButtonStatus = false;
            VersionConvertComboBoxStatus = false;
            ComboBoxStatus = false;
        }
    }

    #endregion

    public async void RowDoubleTappedCommand(object? sender, TappedEventArgs args)
    {
        try
        {
            var source = args.Source;
            if (source is null) return;
            if (source is Border)
            {
                if (DataGridSelectedItem == null)
                    return;

                var workingDirectoryFile =
                    DataGridSelectedItem;

                var filePath = workingDirectoryFile.FilePath;

                if (!File.Exists(filePath) && !Directory.Exists(filePath))
                    throw new DirectoryNotFoundException("Directory was not found");

                if (File.Exists(workingDirectoryFile.FilePath))
                {
                    mainManager.OpenFile(filePath);
                }
                else
                {
                    await mainManager.SetWorkingDirectoryPath(workingDirectoryFile.FilePath);
                    WorkingDirectoryFiles.Clear();
                    await UpdateUiAsync();
                }
            }
        }
        catch (Exception ex)
        {
            var mainWindow = GetMainWindow();
            var messageBox = MessageBoxes.GetErrorBox(ex.Message);

            await MessageBoxManager.GetMessageBoxStandard(messageBox).ShowWindowDialogAsync(mainWindow);
        }
        finally
        {
            ContextOpenFolderStatus = false;
        }
    }

    private void UpdateUIElementsAsync()
    {
        if (DataGridSelectedItem != null)
        {
            var workingDirectoryFile = DataGridSelectedItem;
            var path = workingDirectoryFile.FilePath;
            var extension = Path.GetExtension(path).ToLowerInvariant();

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                ResetUIElements();
                mainManager.RefreshWorkingDirectory();
                UpdateUiAsync().Wait();
                throw new Exception("File or directory do not exist anymore! Refreshing the directory.");
            }

            DebugButtonStatus = extension == ".d3dtx" || extension == ".dds";
            SaveButtonStatus = File.Exists(path);
            DeleteButtonStatus = true;
            ContextOpenFolderStatus = Directory.Exists(path);
            ChooseOutputDirectoryCheckBoxEnabledStatus = true;
            if (extension == string.Empty && !Directory.Exists(path))
            {
                ChangeComboBoxItemsByItemExtension(null);
            }
            else
            {
                ChangeComboBoxItemsByItemExtension(extension);
            }
        }
        else
        {
            ResetUIElements();
        }
    }

    private void ResetUIElements()
    {
        SaveButtonStatus = false;
        DeleteButtonStatus = false;
        DebugButtonStatus = false;
        ConvertButtonStatus = false;
        ComboBoxStatus = false;
        VersionConvertComboBoxStatus = false;
        ChooseOutputDirectoryCheckBoxEnabledStatus = false;
        DebugButtonStatus = false;
        ChooseOutputDirectoryCheckboxStatus = false;

        ImageProperties = ImageData.GetImagePropertiesFromInvalid();
        ImagePreview = new SvgImage()
        {
            Source = SvgSource.Load(ErrorSvgFilename, _assetsUri)
        };
        ImageNamePreview = string.Empty;
    }

    [RelayCommand]
    public async Task UpdateUIElementsOnItemChange()
    {
        await PreviewImage();
        ResetPanAndZoomCommand.Execute(null);
    }

    [RelayCommand]
    public async Task PreviewImage()
    {
        try
        {
            UpdateUIElementsAsync();

            if (DataGridSelectedItem == null)
                return;

            var workingDirectoryFile = DataGridSelectedItem;
            var filePath = workingDirectoryFile.FilePath;
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            ImageNamePreview = workingDirectoryFile.FileName + workingDirectoryFile.FileType;

            TextureType textureType = TextureType.Unknown;
            if (extension != string.Empty)
                textureType = GetTextureTypeFromItem(extension.ToUpperInvariant().Remove(0, 1));

            ImageData imageData = new(filePath, textureType, GetD3DTXConversionType(),MipValue,FaceValue);
            
            MaxMipCount =  imageData.MaxMip;
            MaxFaceCount =  imageData.MaxFace;

            ImageProperties = imageData.ImageProperties;

            if (imageData.ImageBitmap == null)
            {
                ImagePreview = new SvgImage()
                {
                    Source = SvgSource.Load(ErrorSvgFilename, _assetsUri)
                };
            }
            else
            {
                ImagePreview = imageData.ImageBitmap;
            }

            await DebugButton_Click();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            await HandleImagePreviewErrorAsync(ex);
        }
    }

    private Task OpenFileExplorer(string path)
    {
        MainManager.OpenFileExplorer(path);
        return Task.CompletedTask;
    }

    private async Task RefreshUiAsync()
    {
        await SafeRefreshDirectoryAsync();
        await UpdateUiAsync();
    }

    private async Task HandleImagePreviewErrorAsync(Exception ex)
    {
        await HandleExceptionAsync("Error during previewing image.\nError message: " + ex.Message);
        ImagePreview = new SvgImage { Source = SvgSource.Load(ErrorSvgFilename, _assetsUri) };
    }

    private async Task HandleExceptionAsync(string message)
    {
        var mainWindow = GetMainWindow();
        var messageBox = MessageBoxes.GetErrorBox(message);
        await MessageBoxManager.GetMessageBoxStandard(messageBox).ShowWindowDialogAsync(mainWindow);
    }
}