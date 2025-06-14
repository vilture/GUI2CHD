using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace GUI2CHD
{
    public static class ControlExtensions
    {
        public static async Task InvokeAsync(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                await Task.Run(() => control.Invoke(action));
            }
            else
            {
                action();
            }
        }
    }

    public partial class MainForm : Form
    {
        private List<string> pendingFiles = new List<string>();
        private List<string> completedFiles = new List<string>();
        private Process? currentProcess;
        private CancellationTokenSource? cancellationTokenSource;
        private bool isConverting = false;
        private TextBox detailedLogBox;
        private Settings settings;
        private ProgressBar extractProgressBar;
        private Label lblExtractProgress;
        private ListBox pendingList;
        private ListBox completedList;
        private TextBox logBox;
        private ProgressBar progressBar;
        private Label lblOutputFolder;
        private Label lblPending;
        private Label lblCompletedList;
        private Label lblMainLog;
        private Label lblDetailedLog;

        private readonly Dictionary<string, Dictionary<string, string>> translations = new()
        {
            ["ru"] = new Dictionary<string, string>
            {
                ["SelectFiles"] = "Выбрать файлы",
                ["Convert"] = "Конвертировать",
                ["Stop"] = "Остановить",
                ["Pending"] = "Ожидающие конвертации:",
                ["Completed"] = "Успешно сконвертированные:",
                ["MainLog"] = "Основной лог:",
                ["DetailedLog"] = "Подробный лог chdman.exe:",
                ["OutputFolder"] = "Папка не выбрана",
                ["MenuSettings"] = "Настройки",
                ["MenuPaths"] = "Пути",
                ["MenuHelp"] = "Справка",
                ["MenuAbout"] = "О программе",
                ["MenuLanguage"] = "Язык",
                ["LangRu"] = "Русский",
                ["LangEn"] = "English",
                ["LangZh"] = "简体中文",
                ["Progress"] = "Прогресс конвертации:",
                ["SettingsTitle"] = "Настройки путей",
                ["OutputFolderLabel"] = "Папка для CHD файлов:",
                ["TempFolderLabel"] = "Папка для временных файлов:",
                ["Browse"] = "Обзор",
                ["OK"] = "OK",
                ["Cancel"] = "Отмена",
                ["ErrorNoOutputFolder"] = "Необходимо указать папку для CHD файлов!",
                ["Error"] = "Ошибка: {0}",
                ["ExtractingArchive"] = "Распаковка архива",
                ["Converting"] = "Конвертация {0} в {1}...",
                ["Converted"] = "Сконвертированные: {0}",
                ["ConversionStopped"] = "Конвертация остановлена пользователем.",
                ["AboutTitle"] = "О программе",
                ["AboutVersion"] = "Версия 1.0",
                ["AboutAuthor"] = "Автор: Батухтин Артем",
                ["AboutLicense"] = "Лицензия: MIT License",
                ["AboutDisclaimer"] = "Программа распространяется бесплатно и без каких-либо обязательств.\nАвтор не несет ответственности за любой возможный ущерб, связанный с использованием программы.",
                ["AboutComponents"] = "Используемые компоненты:",
                ["AboutComponent1"] = "1. chdman\n   - Часть проекта MAME\n   - Лицензия: BSD-3-Clause\n   - https://www.mamedev.org/",
                ["AboutComponent2"] = "2. 7-Zip\n   - Автор: Igor Pavlov\n   - Лицензия: GNU LGPL\n   - https://www.7-zip.org/",
                ["AboutComponent3"] = "3. ccd2iso\n   - Конвертер образов CloneCD в ISO\n   - Лицензия: GNU GPL v2\n   - https://github.com/jkmartindale/ccd2iso",
                ["AboutComponent4"] = "4. .NET 6.0\n   - Microsoft Corporation\n   - Лицензия: MIT\n   - https://dotnet.microsoft.com/",
                ["AboutLegalNotice"] = "Отказ от ответственности:\nДанная программа предназначена для конвертации легально приобретенных образов дисков в формат CHD.\nПользователь несет полную ответственность за соблюдение авторских прав при использовании программы.",
                ["MITLicense"] = @"MIT License

Данная лицензия разрешает любому лицу, получившему копию данного программного обеспечения и связанных с ним файлов документации (""Программное обеспечение""), безвозмездно использовать Программное обеспечение без ограничений, включая, без ограничений, права на использование, копирование, изменение, объединение, публикацию, распространение, сублицензирование и/или продажу копий Программного обеспечения, а также лицам, которым предоставляется Программное обеспечение, при соблюдении следующих условий:

Вышеуказанное уведомление об авторском праве и данное уведомление о разрешении должны быть включены во все копии или существенные части Программного обеспечения.

ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ ПРЕДОСТАВЛЯЕТСЯ ""КАК ЕСТЬ"", БЕЗ КАКИХ-ЛИБО ГАРАНТИЙ, ЯВНЫХ ИЛИ ПОДРАЗУМЕВАЕМЫХ, ВКЛЮЧАЯ, НО НЕ ОГРАНИЧИВАЯСЬ, ГАРАНТИИ ТОВАРНОЙ ПРИГОДНОСТИ, СООТВЕТСТВИЯ ПО ОПРЕДЕЛЕННОМУ НАЗНАЧЕНИЮ И НЕНАРУШЕНИЯ ПРАВ ТРЕТЬИХ ЛИЦ. НИ В КАКОМ СЛУЧАЕ АВТОРЫ ИЛИ ОБЛАДАТЕЛИ АВТОРСКИХ ПРАВ НЕ НЕСУТ ОТВЕТСТВЕННОСТИ ЗА ЛЮБЫЕ ПРЕТЕНЗИИ, УБЫТКИ ИЛИ ИНЫЕ ОБЯЗАТЕЛЬСТВА, БУДЬ ТО В РЕЗУЛЬТАТЕ ДОГОВОРА, ДЕЛИКТА ИЛИ ИНОГО, ВОЗНИКШИЕ ИЗ, ИМЕЮЩИЕ ПРИЧИНОЙ ИЛИ СВЯЗАННЫЕ С ПРОГРАММНЫМ ОБЕСПЕЧЕНИЕМ ИЛИ ИСПОЛЬЗОВАНИЕМ ИЛИ ИНЫМИ ДЕЙСТВИЯМИ В ПРОГРАММНОМ ОБЕСПЕЧЕНИИ."
            },
            ["en"] = new Dictionary<string, string>
            {
                ["SelectFiles"] = "Select Files",
                ["Convert"] = "Convert",
                ["Stop"] = "Stop",
                ["Pending"] = "Pending Conversion:",
                ["Completed"] = "Successfully Converted:",
                ["MainLog"] = "Main Log:",
                ["DetailedLog"] = "Detailed chdman.exe Log:",
                ["OutputFolder"] = "Folder not selected",
                ["MenuSettings"] = "Settings",
                ["MenuPaths"] = "Paths",
                ["MenuHelp"] = "Help",
                ["MenuAbout"] = "About",
                ["MenuLanguage"] = "Language",
                ["LangRu"] = "Русский",
                ["LangEn"] = "English",
                ["LangZh"] = "简体中文",
                ["Progress"] = "Conversion Progress:",
                ["SettingsTitle"] = "Path Settings",
                ["OutputFolderLabel"] = "CHD Output Folder:",
                ["TempFolderLabel"] = "Temporary Files Folder:",
                ["Browse"] = "Browse",
                ["OK"] = "OK",
                ["Cancel"] = "Cancel",
                ["ErrorNoOutputFolder"] = "You must specify a folder for CHD files!",
                ["Error"] = "Error: {0}",
                ["ExtractingArchive"] = "Extracting archive",
                ["Converting"] = "Converting {0} to {1}...",
                ["Converted"] = "Converted: {0}",
                ["ConversionStopped"] = "Conversion stopped by user.",
                ["AboutTitle"] = "About",
                ["AboutVersion"] = "Version 1.0",
                ["AboutAuthor"] = "Author: Artem Batukhtin",
                ["AboutLicense"] = "License: MIT License",
                ["AboutDisclaimer"] = "This program is distributed free of charge and without any obligations.\nThe author is not responsible for any possible damage related to the use of the program.",
                ["AboutComponents"] = "Used components:",
                ["AboutComponent1"] = "1. chdman\n   - Part of MAME project\n   - License: BSD-3-Clause\n   - https://www.mamedev.org/",
                ["AboutComponent2"] = "2. 7-Zip\n   - Author: Igor Pavlov\n   - License: GNU LGPL\n   - https://www.7-zip.org/",
                ["AboutComponent3"] = "3. ccd2iso\n   - CloneCD to ISO converter\n   - License: GNU GPL v2\n   - https://github.com/jkmartindale/ccd2iso",
                ["AboutComponent4"] = "4. .NET 6.0\n   - Microsoft Corporation\n   - License: MIT\n   - https://dotnet.microsoft.com/",
                ["AboutLegalNotice"] = "Legal Notice:\nThis program is intended for converting legally acquired disk images to CHD format.\nThe user is fully responsible for compliance with copyright laws when using the program.",
                ["MITLicense"] = @"MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
            },
            ["zh"] = new Dictionary<string, string>
            {
                ["SelectFiles"] = "选择文件",
                ["Convert"] = "转换",
                ["Stop"] = "停止",
                ["Pending"] = "待转换：",
                ["Completed"] = "转换成功：",
                ["MainLog"] = "主日志：",
                ["DetailedLog"] = "chdman.exe 详细日志：",
                ["OutputFolder"] = "未选择文件夹",
                ["MenuSettings"] = "设置",
                ["MenuPaths"] = "路径",
                ["MenuHelp"] = "帮助",
                ["MenuAbout"] = "关于",
                ["MenuLanguage"] = "语言",
                ["LangRu"] = "Русский",
                ["LangEn"] = "English",
                ["LangZh"] = "简体中文",
                ["Progress"] = "转换进度：",
                ["SettingsTitle"] = "路径设置",
                ["OutputFolderLabel"] = "CHD文件夹:",
                ["TempFolderLabel"] = "临时文件夹:",
                ["Browse"] = "浏览",
                ["OK"] = "确定",
                ["Cancel"] = "取消",
                ["ErrorNoOutputFolder"] = "必须指定CHD文件夹!",
                ["Error"] = "错误: {0}",
                ["ExtractingArchive"] = "正在解压归档",
                ["Converting"] = "正在转换 {0} 到 {1}...",
                ["Converted"] = "已转换: {0}",
                ["ConversionStopped"] = "用户已停止转换。",
                ["AboutTitle"] = "关于",
                ["AboutVersion"] = "版本 1.0",
                ["AboutAuthor"] = "作者：Artem Batukhtin",
                ["AboutLicense"] = "许可证：MIT License",
                ["AboutDisclaimer"] = "本程序免费分发，不承担任何义务。\n作者不对使用本程序可能造成的任何损害负责。",
                ["AboutComponents"] = "使用的组件：",
                ["AboutComponent1"] = "1. chdman\n   - Part of MAME project\n   - License: BSD-3-Clause\n   - https://www.mamedev.org/",
                ["AboutComponent2"] = "2. 7-Zip\n   - Author: Igor Pavlov\n   - License: GNU LGPL\n   - https://www.7-zip.org/",
                ["AboutComponent3"] = "3. ccd2iso\n   - CloneCD to ISO converter\n   - License: GNU GPL v2\n   - https://github.com/jkmartindale/ccd2iso",
                ["AboutComponent4"] = "4. .NET 6.0\n   - Microsoft Corporation\n   - License: MIT\n   - https://dotnet.microsoft.com/",
                ["AboutLegalNotice"] = "法律声明：\n本程序用于将合法获取的磁盘镜像转换为CHD格式。\n用户在使用本程序时需完全遵守版权法。",
                ["MITLicense"] = @"MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
            }
        };

        public MainForm()
        {
            settings = Settings.Load();
            if (!Directory.Exists(settings.TempFolder))
            {
                Directory.CreateDirectory(settings.TempFolder);
            }
            InitializeControls();
            this.Resize += MainForm_Resize;
            
            // Устанавливаем иконку формы
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new System.Drawing.Icon(iconPath);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Обновляем размеры элементов управления при изменении размера формы
            int rightMargin = 20;
            int bottomMargin = 20;
            int buttonHeight = 30;
            int labelHeight = 20;
            int listBoxSpacing = 20;
            int logBoxSpacing = 20;
            int menuHeight = this.MainMenuStrip.Height;

            // Обновляем размеры списков
            int listBoxWidth = (this.ClientSize.Width - rightMargin * 2) / 2;
            int listBoxHeight = (this.ClientSize.Height - menuHeight - buttonHeight - labelHeight * 4 - listBoxSpacing * 2 - bottomMargin) / 2;

            // Обновляем позиции кнопок
            Button btnSelectFiles = this.Controls.OfType<Button>().First(b => b.Name == "btnSelectFiles");
            Button btnConvert = this.Controls.OfType<Button>().First(b => b.Name == "btnConvert");
            Button btnStop = this.Controls.OfType<Button>().First(b => b.Name == "btnStop");

            btnSelectFiles.Location = new Point(rightMargin, menuHeight + 10);
            btnConvert.Location = new Point(rightMargin + btnSelectFiles.Width + 10, menuHeight + 10);
            btnStop.Location = new Point(rightMargin + btnSelectFiles.Width + btnConvert.Width + 20, menuHeight + 10);

            // Обновляем позицию и размер метки с путем
            lblOutputFolder.Location = new Point(rightMargin + btnSelectFiles.Width + btnConvert.Width + btnStop.Width + 30, menuHeight + 15);
            lblOutputFolder.Size = new Size(this.ClientSize.Width - lblOutputFolder.Location.X - rightMargin, labelHeight);

            // Обновляем позиции и размеры списков
            int currentY = menuHeight + buttonHeight + 20;

            lblPending.Location = new Point(rightMargin, currentY);
            currentY += labelHeight + 5;
            pendingList.Location = new Point(rightMargin, currentY);
            pendingList.Size = new Size(listBoxWidth, listBoxHeight);

            currentY += listBoxHeight + listBoxSpacing;
            lblCompletedList.Location = new Point(rightMargin, currentY);
            currentY += labelHeight + 5;
            completedList.Location = new Point(rightMargin, currentY);
            completedList.Size = new Size(listBoxWidth, listBoxHeight);

            // Обновляем позиции и размеры логов
            lblMainLog.Location = new Point(rightMargin + listBoxWidth + rightMargin, menuHeight + buttonHeight + 20);
            logBox.Location = new Point(rightMargin + listBoxWidth + rightMargin, menuHeight + buttonHeight + 20 + labelHeight + 5);
            logBox.Size = new Size(listBoxWidth, listBoxHeight);

            lblDetailedLog.Location = new Point(rightMargin + listBoxWidth + rightMargin, menuHeight + buttonHeight + 20 + labelHeight + 5 + listBoxHeight + listBoxSpacing);
            detailedLogBox.Location = new Point(rightMargin + listBoxWidth + rightMargin, menuHeight + buttonHeight + 20 + labelHeight + 5 + listBoxHeight + listBoxSpacing + labelHeight + 5);
            detailedLogBox.Size = new Size(listBoxWidth, listBoxHeight);

            // Обновляем позиции и размеры прогресс-бара
            lblExtractProgress.Location = new Point(rightMargin + listBoxWidth + rightMargin, menuHeight + 35);
            extractProgressBar.Location = new Point(rightMargin + listBoxWidth + rightMargin, menuHeight + 60);
            extractProgressBar.Size = new Size(listBoxWidth, 23);
        }

        private void InitializeControls()
        {
            // Настройка формы
            this.Text = "GUI2CHD - Конвертер образов в CHD";
            this.Size = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(800, 600);

            // Создание меню
            MenuStrip menuStrip = new MenuStrip();
            
            // Меню "Настройки"
            ToolStripMenuItem settingsMenu = new ToolStripMenuItem("Настройки");
            ToolStripMenuItem pathsMenuItem = new ToolStripMenuItem("Пути");
            pathsMenuItem.Click += PathsMenuItem_Click;
            settingsMenu.DropDownItems.Add(pathsMenuItem);
            menuStrip.Items.Add(settingsMenu);

            // Меню "Язык"
            ToolStripMenuItem languageMenu = new ToolStripMenuItem("Язык");
            ToolStripMenuItem langRu = new ToolStripMenuItem("Русский") { Tag = "ru" };
            ToolStripMenuItem langEn = new ToolStripMenuItem("English") { Tag = "en" };
            ToolStripMenuItem langZh = new ToolStripMenuItem("简体中文") { Tag = "zh" };
            langRu.Click += LanguageMenuItem_Click;
            langEn.Click += LanguageMenuItem_Click;
            langZh.Click += LanguageMenuItem_Click;
            languageMenu.DropDownItems.AddRange(new[] { langRu, langEn, langZh });
            menuStrip.Items.Add(languageMenu);

            // Меню "Справка"
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");
            ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("О программе");
            aboutMenuItem.Click += AboutMenuItem_Click;
            helpMenu.DropDownItems.Add(aboutMenuItem);
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Создание элементов управления
            Button btnSelectFiles = new Button
            {
                Name = "btnSelectFiles",
                Text = "Выбрать файлы",
                Location = new System.Drawing.Point(10, 30),
                Size = new System.Drawing.Size(120, 30)
            };
            btnSelectFiles.Click += BtnSelectFiles_Click;

            Button btnConvert = new Button
            {
                Name = "btnConvert",
                Text = "Конвертировать",
                Location = new System.Drawing.Point(140, 30),
                Size = new System.Drawing.Size(150, 30),
                Enabled = false
            };
            btnConvert.Click += BtnConvert_Click;

            Button btnStop = new Button
            {
                Name = "btnStop",
                Text = "Остановить",
                Location = new System.Drawing.Point(300, 30),
                Size = new System.Drawing.Size(120, 30),
                Enabled = false
            };
            btnStop.Click += BtnStop_Click;

            lblOutputFolder = new Label
            {
                Name = "lblOutputFolder",
                Text = settings.OutputFolder == string.Empty ? "Папка не выбрана" : settings.OutputFolder,
                Location = new System.Drawing.Point(400, 35),
                Size = new System.Drawing.Size(240, 20),
                AutoEllipsis = true
            };

            lblPending = new Label
            {
                Name = "lblPending",
                Text = "Ожидающие конвертации:",
                Location = new System.Drawing.Point(10, 70),
                Size = new System.Drawing.Size(200, 20)
            };

            pendingList = new ListBox
            {
                Name = "pendingList",
                Location = new System.Drawing.Point(10, 90),
                Size = new System.Drawing.Size(480, 300),
                AllowDrop = true
            };
            pendingList.DragEnter += PendingList_DragEnter;
            pendingList.DragDrop += PendingList_DragDrop;
            pendingList.DragLeave += PendingList_DragLeave;
            pendingList.KeyDown += PendingList_KeyDown;

            lblCompletedList = new Label
            {
                Name = "lblCompletedList",
                Text = "Успешно сконвертированные:",
                Location = new System.Drawing.Point(320, 10),
                Size = new System.Drawing.Size(180, 20),
                AutoSize = true
            };

            completedList = new ListBox
            {
                Name = "completedList",
                Location = new System.Drawing.Point(10, 420),
                Size = new System.Drawing.Size(480, 250)
            };

            lblMainLog = new Label
            {
                Name = "lblMainLog",
                Text = "Основной лог:",
                Location = new System.Drawing.Point(500, 120),
                Size = new System.Drawing.Size(200, 20)
            };

            logBox = new TextBox
            {
                Name = "logBox",
                Location = new System.Drawing.Point(500, 140),
                Size = new System.Drawing.Size(680, 200),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            lblDetailedLog = new Label
            {
                Name = "lblDetailedLog",
                Text = "Подробный лог chdman.exe:",
                Location = new System.Drawing.Point(500, 350),
                Size = new System.Drawing.Size(200, 20)
            };

            detailedLogBox = new TextBox
            {
                Name = "detailedLogBox",
                Location = new System.Drawing.Point(500, 370),
                Size = new System.Drawing.Size(680, 300),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            lblExtractProgress = new Label
            {
                Name = "lblExtractProgress",
                Text = "Прогресс конвертации:",
                Location = new System.Drawing.Point(500, 60),
                Size = new System.Drawing.Size(200, 20),
                Visible = false
            };

            extractProgressBar = new ProgressBar
            {
                Name = "extractProgressBar",
                Location = new System.Drawing.Point(500, 80),
                Size = new System.Drawing.Size(680, 23),
                Visible = false
            };

            // Добавление элементов на форму
            this.Controls.AddRange(new Control[] { 
                btnSelectFiles, btnConvert, btnStop,
                lblOutputFolder, lblPending, pendingList, lblCompletedList, completedList,
                lblMainLog, logBox, lblDetailedLog, detailedLogBox,
                lblExtractProgress, extractProgressBar
            });

            // Вызываем обработчик изменения размера для начальной настройки
            MainForm_Resize(this, EventArgs.Empty);
        }

        private void PathsMenuItem_Click(object sender, EventArgs e)
        {
            var t = translations.ContainsKey(settings.Language) ? translations[settings.Language] : translations["ru"];
            using (Form settingsForm = new Form())
            {
                settingsForm.Text = t["SettingsTitle"];
                settingsForm.Size = new System.Drawing.Size(600, 250);
                settingsForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                settingsForm.MaximizeBox = false;
                settingsForm.MinimizeBox = false;
                settingsForm.StartPosition = FormStartPosition.CenterParent;
                settingsForm.Padding = new Padding(10);
                settingsForm.Icon = this.Icon;

                Label lblOutputFolder = new Label
                {
                    Text = t["OutputFolderLabel"],
                    Location = new System.Drawing.Point(10, 15),
                    Size = new System.Drawing.Size(250, 20),
                    AutoSize = true
                };

                TextBox txtOutputFolder = new TextBox
                {
                    Text = settings.OutputFolder,
                    Location = new System.Drawing.Point(10, 40),
                    Size = new System.Drawing.Size(450, 23),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                Button btnBrowseOutput = new Button
                {
                    Text = t["Browse"],
                    Location = new System.Drawing.Point(470, 39),
                    Size = new System.Drawing.Size(100, 25),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnBrowseOutput.Click += (s, ev) =>
                {
                    using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                    {
                        dialog.SelectedPath = txtOutputFolder.Text;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            txtOutputFolder.Text = dialog.SelectedPath;
                        }
                    }
                };

                Label lblTempFolder = new Label
                {
                    Text = t["TempFolderLabel"],
                    Location = new System.Drawing.Point(10, 75),
                    Size = new System.Drawing.Size(250, 20),
                    AutoSize = true
                };

                TextBox txtTempFolder = new TextBox
                {
                    Text = settings.TempFolder,
                    Location = new System.Drawing.Point(10, 100),
                    Size = new System.Drawing.Size(450, 23),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                Button btnBrowseTemp = new Button
                {
                    Text = t["Browse"],
                    Location = new System.Drawing.Point(470, 99),
                    Size = new System.Drawing.Size(100, 25),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnBrowseTemp.Click += (s, ev) =>
                {
                    using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                    {
                        dialog.SelectedPath = txtTempFolder.Text;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            txtTempFolder.Text = dialog.SelectedPath;
                        }
                    }
                };

                Button btnOK = new Button
                {
                    Text = t["OK"],
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(380, 160),
                    Size = new System.Drawing.Size(90, 30),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };
                btnOK.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtOutputFolder.Text))
                    {
                        MessageBox.Show(t["ErrorNoOutputFolder"], t["Error"], MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    settings.OutputFolder = txtOutputFolder.Text;
                    settings.TempFolder = string.IsNullOrWhiteSpace(txtTempFolder.Text) ? 
                        Path.GetTempPath() : txtTempFolder.Text;
                    
                    if (!Directory.Exists(settings.OutputFolder))
                    {
                        Directory.CreateDirectory(settings.OutputFolder);
                    }
                    if (!Directory.Exists(settings.TempFolder))
                    {
                        Directory.CreateDirectory(settings.TempFolder);
                    }
                    
                    settings.Save();
                    
                    // Обновляем метку с путем на главной форме
                    lblOutputFolder = this.Controls.OfType<Label>().First(l => l.Name == "lblOutputFolder");
                    lblOutputFolder.Text = settings.OutputFolder;
                    UpdateConvertButtonState();
                };

                Button btnCancel = new Button
                {
                    Text = t["Cancel"],
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(480, 160),
                    Size = new System.Drawing.Size(90, 30),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };

                settingsForm.Controls.AddRange(new Control[] { 
                    lblOutputFolder, txtOutputFolder, btnBrowseOutput,
                    lblTempFolder, txtTempFolder, btnBrowseTemp,
                    btnOK, btnCancel 
                });

                settingsForm.AcceptButton = btnOK;
                settingsForm.CancelButton = btnCancel;

                settingsForm.ShowDialog();
            }
        }

        private void BtnSelectFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Образы дисков и архивы|*.iso;*.cue;*.gdi;*.ccd;*.7z;*.zip;*.rar|Все файлы|*.*";
                openFileDialog.Multiselect = true;
                
                if (!string.IsNullOrEmpty(settings.LastInputFilesFolder) && Directory.Exists(settings.LastInputFilesFolder))
                {
                    openFileDialog.InitialDirectory = settings.LastInputFilesFolder;
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (openFileDialog.FileNames.Length > 0)
                    {
                        settings.LastInputFilesFolder = Path.GetDirectoryName(openFileDialog.FileNames[0]) ?? string.Empty;
                        settings.Save();
                    }
                    ProcessFiles(openFileDialog.FileNames);
                }
            }
        }

        private void ProcessFiles(string[] files)
        {
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".7z" || extension == ".zip" || extension == ".rar")
                {
                    _ = ProcessArchiveAsync(file);
                }
                else if (extension == ".iso" || extension == ".cue" || extension == ".ccd" || extension == ".gdi")
                {
                    // Для CCD файлов проверяем наличие связанных файлов
                    if (extension == ".ccd")
                    {
                        string directory = Path.GetDirectoryName(file) ?? string.Empty;
                        string baseName = Path.GetFileNameWithoutExtension(file);
                        string imgFile = Path.Combine(directory, baseName + ".img");
                        
                        if (!File.Exists(imgFile))
                        {
                            MessageBox.Show($"Для файла {Path.GetFileName(file)} не найден связанный файл образа {Path.GetFileName(imgFile)}", 
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                    }
                    // Для GDI файлов проверяем наличие связанных файлов
                    else if (extension == ".gdi")
                    {
                        string directory = Path.GetDirectoryName(file) ?? string.Empty;
                        string baseName = Path.GetFileNameWithoutExtension(file);
                        string binFile = Path.Combine(directory, baseName + ".bin");
                        
                        if (!File.Exists(binFile))
                        {
                            MessageBox.Show($"Для файла {Path.GetFileName(file)} не найден связанный файл образа {Path.GetFileName(binFile)}", 
                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }
                    }
                    AddFilesToList(new[] { file });
                }
            }
        }

        private List<string> GetAssociatedFiles(string file)
        {
            var filesToDelete = new List<string> { file };
            
            if (file.EndsWith(".cue", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string cueContent = File.ReadAllText(file);
                    string directory = Path.GetDirectoryName(file) ?? string.Empty;
                    
                    // Ищем все строки с FILE в CUE-файле
                    foreach (string line in cueContent.Split('\n'))
                    {
                        if (line.Trim().StartsWith("FILE", StringComparison.OrdinalIgnoreCase))
                        {
                            // Извлекаем путь к файлу из строки FILE
                            int startQuote = line.IndexOf('"');
                            int endQuote = line.LastIndexOf('"');
                            if (startQuote != -1 && endQuote != -1)
                            {
                                string binFile = line.Substring(startQuote + 1, endQuote - startQuote - 1);
                                string fullBinPath = Path.Combine(directory, binFile);
                                if (File.Exists(fullBinPath))
                                {
                                    filesToDelete.Add(fullBinPath);
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            else if (file.EndsWith(".gdi", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string gdiContent = File.ReadAllText(file);
                    string directory = Path.GetDirectoryName(file) ?? string.Empty;
                    
                    // Пропускаем первую строку (количество треков)
                    string[] lines = gdiContent.Split('\n').Skip(1).ToArray();
                    
                    foreach (string line in lines)
                    {
                        string[] parts = line.Trim().Split(' ');
                        if (parts.Length >= 4)
                        {
                            // В GDI файле формат: track_number start_sector sector_type filename
                            string binFile = parts[3].Trim('"');
                            string fullBinPath = Path.Combine(directory, binFile);
                            if (File.Exists(fullBinPath))
                            {
                                filesToDelete.Add(fullBinPath);
                            }
                        }
                    }
                }
                catch { }
            }
            
            return filesToDelete;
        }

        private bool CheckGdiAssociatedFiles(string gdiFile)
        {
            // Убираем проверку, так как chdman сам проверяет зависимости
            return true;
        }

        private async Task ProcessArchiveAsync(string archivePath)
        {
            var t = translations.ContainsKey(settings.Language) ? translations[settings.Language] : translations["ru"];
            ExtractProgressForm progressForm = null;
            try
            {
                string archiveName = Path.GetFileName(archivePath);
                string extractPath = Path.Combine(settings.TempFolder, Path.GetFileNameWithoutExtension(archivePath));
                
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);

                string sevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7za", "7za.exe");
                if (!File.Exists(sevenZipPath))
                {
                    throw new FileNotFoundException($"Не найден файл 7za.exe по пути: {sevenZipPath}");
                }

                // Показываем модальное окно прогресса
                await this.InvokeAsync(() =>
                {
                    progressForm = new ExtractProgressForm(archiveName);
                    progressForm.Text = t["ExtractingArchive"];
                    progressForm.Show(this);
                    progressForm.Refresh();
                });

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = $"x \"{archivePath}\" -o\"{extractPath}\" -y",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        while (!process.StandardOutput.EndOfStream)
                        {
                            string? line = await process.StandardOutput.ReadLineAsync();
                            if (line != null && progressForm != null)
                            {
                                // Пытаемся извлечь процент из строки
                                if (line.Contains("%"))
                                {
                                    string[] parts = line.Split('%');
                                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int percent))
                                    {
                                        await progressForm.InvokeAsync(() =>
                                        {
                                            progressForm.ProgressBar.Value = Math.Min(percent, 100);
                                        });
                                    }
                                }
                            }
                        }

                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0)
                        {
                            // Ищем все поддерживаемые файлы образов
                            string[] foundFiles = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories)
                                .Where(f => f.EndsWith(".iso", StringComparison.OrdinalIgnoreCase) || 
                                          f.EndsWith(".cue", StringComparison.OrdinalIgnoreCase) ||
                                          f.EndsWith(".ccd", StringComparison.OrdinalIgnoreCase) ||
                                          f.EndsWith(".gdi", StringComparison.OrdinalIgnoreCase))
                                .ToArray();

                            // Проверяем наличие связанных файлов только для CCD
                            var validFiles = new List<string>();
                            foreach (string file in foundFiles)
                            {
                                string extension = Path.GetExtension(file).ToLower();
                                if (extension == ".ccd")
                                {
                                    string directory = Path.GetDirectoryName(file) ?? string.Empty;
                                    string baseName = Path.GetFileNameWithoutExtension(file);
                                    string imgFile = Path.Combine(directory, baseName + ".img");
                                    
                                    if (File.Exists(imgFile))
                                    {
                                        validFiles.Add(file);
                                    }
                                    else
                                    {
                                        await this.InvokeAsync(() =>
                                        {
                                            MessageBox.Show($"Для файла {Path.GetFileName(file)} не найден связанный файл образа {Path.GetFileName(imgFile)}", 
                                                "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        });
                                    }
                                }
                                else
                                {
                                    validFiles.Add(file);
                                }
                            }

                            if (validFiles.Count > 0)
                            {
                                AddFilesToList(validFiles.ToArray());
                            }
                        }
                        else
                        {
                            throw new Exception($"Ошибка при распаковке архива. Код выхода: {process.ExitCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await this.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при обработке архива {Path.GetFileName(archivePath)}: {ex.Message}", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
            finally
            {
                if (progressForm != null)
                {
                    await progressForm.InvokeAsync(() => progressForm.Close());
                }
            }
        }

        private void AddFilesToList(string[] files)
        {
            foreach (string file in files)
            {
                if (!pendingFiles.Contains(file) && !completedFiles.Contains(file))
                {
                    pendingFiles.Add(file);
                }
            }
            UpdateFileLists();
        }

        private void UpdateFileLists()
        {
            ListBox pendingList = this.Controls.OfType<ListBox>().First(l => l.Name == "pendingList");
            ListBox completedList = this.Controls.OfType<ListBox>().First(l => l.Name == "completedList");
            
            pendingList.Items.Clear();
            pendingList.Items.AddRange(pendingFiles.ToArray());
            
            completedList.Items.Clear();
            completedList.Items.AddRange(completedFiles.ToArray());
            
            UpdateConvertButtonState();
        }

        private void UpdateConvertButtonState()
        {
            Button btnConvert = this.Controls.OfType<Button>().First(b => b.Name == "btnConvert");
            btnConvert.Enabled = pendingFiles.Count > 0 && !string.IsNullOrWhiteSpace(settings.OutputFolder);
        }

        private async void BtnConvert_Click(object sender, EventArgs e)
        {
            if (isConverting) return;

            Button btnConvert = (Button)sender;
            Button btnStop = this.Controls.OfType<Button>().First(b => b.Name == "btnStop");
            btnConvert.Enabled = false;
            btnStop.Enabled = true;
            isConverting = true;

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            try
            {
                foreach (string file in pendingFiles.ToList())
                {
                    if (token.IsCancellationRequested)
                        break;

                    string outputFile = Path.Combine(settings.OutputFolder, Path.GetFileNameWithoutExtension(file) + ".chd");
                    logBox.AppendText(string.Format(translations[settings.Language]["Converting"], Path.GetFileName(file), outputFile) + "\r\n");

                try
                {
                    await ConvertToCHD(file, outputFile);
                        
                        pendingFiles.Remove(file);
                        completedFiles.Add(file);
                        logBox.AppendText(string.Format(translations[settings.Language]["Converted"], Path.GetFileName(outputFile)) + "\r\n");

                        // Удаляем все связанные файлы, если они из временной папки
                        if (file.StartsWith(settings.TempFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            var filesToDelete = GetAssociatedFiles(file);
                            foreach (string fileToDelete in filesToDelete)
                            {
                                try 
                                { 
                                    if (File.Exists(fileToDelete))
                                    {
                                        File.Delete(fileToDelete);
                                    }
                                } 
                                catch (Exception ex)
                                {
                                    logBox.AppendText(string.Format(translations[settings.Language]["Error"], $"Ошибка при удалении {Path.GetFileName(fileToDelete)}: {ex.Message}") + "\r\n");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logBox.AppendText(string.Format(translations[settings.Language]["Error"], ex.Message) + "\r\n");
                    }

                    UpdateFileLists();
                }
            }
            catch (OperationCanceledException)
            {
                logBox.AppendText(translations[settings.Language]["ConversionStopped"] + "\r\n");
            }
            finally
            {
                btnConvert.Enabled = true;
                btnStop.Enabled = false;
                isConverting = false;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                currentProcess = null;
            }
        }

        private async Task<string?> ConvertCCDToISO(string ccdFile)
        {
            string directory = Path.GetDirectoryName(ccdFile) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(ccdFile);
            string isoFile = Path.Combine(directory, baseName + ".iso");

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "ccd2iso.exe",
                    Arguments = $"\"{ccdFile}\" \"{isoFile}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        if (process.ExitCode == 0 && File.Exists(isoFile))
                        {
                            return isoFile;
                        }
                        else
                        {
                            throw new Exception($"Ошибка конвертации CCD в ISO. Код выхода: {process.ExitCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при конвертации CCD в ISO: {ex.Message}");
            }

            return null;
        }

        private Task ConvertToCHD(string inputFile, string outputFile)
        {
            return Task.Run(() =>
            {
                string arguments;
                if (inputFile.EndsWith(".ccd", StringComparison.OrdinalIgnoreCase))
                {
                    // Для CloneCD сначала конвертируем в ISO
                    string directory = Path.GetDirectoryName(inputFile) ?? string.Empty;
                    string baseName = Path.GetFileNameWithoutExtension(inputFile);
                    string isoFile = Path.Combine(directory, baseName + ".iso");
                    
                    // Конвертируем CCD в ISO
                    string ccd2isoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "convert", "ccd2iso.exe");
                    if (!File.Exists(ccd2isoPath))
                    {
                        throw new FileNotFoundException($"Не найден файл конвертера: {ccd2isoPath}");
                    }

                    ProcessStartInfo ccd2isoStartInfo = new ProcessStartInfo
                    {
                        FileName = ccd2isoPath,
                        Arguments = $"\"{inputFile}\" \"{isoFile}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (Process ccd2isoProcess = Process.Start(ccd2isoStartInfo))
                    {
                        if (ccd2isoProcess != null)
                        {
                            // Читаем вывод конвертера
                            while (!ccd2isoProcess.StandardOutput.EndOfStream)
                            {
                                string? line = ccd2isoProcess.StandardOutput.ReadLine();
                                if (line != null)
                                {
                                    detailedLogBox.Invoke((MethodInvoker)(() =>
                                    {
                                        detailedLogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] [ccd2iso] {line}\r\n");
                                        detailedLogBox.ScrollToCaret();
                                    }));
                                }
                            }

                            ccd2isoProcess.WaitForExit();
                            if (ccd2isoProcess.ExitCode != 0)
                            {
                                throw new Exception($"Ошибка конвертации CCD в ISO. Код выхода: {ccd2isoProcess.ExitCode}");
                            }
                        }
                    }

                    // Теперь конвертируем ISO в CHD
                    arguments = $"createcd -i \"{isoFile}\" -o \"{outputFile}\" --force";
                }
                else
                {
                    // Для всех остальных форматов (включая GDI) используем стандартный вызов
                    arguments = $"createcd -i \"{inputFile}\" -o \"{outputFile}\" --force";
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "convert", "chdman.exe"),
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                currentProcess = Process.Start(startInfo);
                if (currentProcess != null)
                {
                    // Чтение вывода в отдельном потоке
                    Task.Run(() =>
                    {
                        while (!currentProcess.StandardOutput.EndOfStream)
                        {
                            string? line = currentProcess.StandardOutput.ReadLine();
                            if (line != null)
                            {
                                detailedLogBox.Invoke((MethodInvoker)(() =>
                                {
                                    detailedLogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}\r\n");
                                    detailedLogBox.ScrollToCaret();
                                }));
                            }
                        }
                    });

                    // Чтение ошибок в отдельном потоке
                    Task.Run(() =>
                    {
                        while (!currentProcess.StandardError.EndOfStream)
                        {
                            string? line = currentProcess.StandardError.ReadLine();
                            if (line != null)
                            {
                                detailedLogBox.Invoke((MethodInvoker)(() =>
                                {
                                    detailedLogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}\r\n");
                                    detailedLogBox.ScrollToCaret();
                                }));
                            }
                        }
                    });

                    currentProcess.WaitForExit();
                    if (currentProcess.ExitCode != 0)
                    {
                        throw new Exception($"Ошибка конвертации. Код выхода: {currentProcess.ExitCode}");
                    }

                    // Удаляем временный ISO файл, если он был создан
                    if (inputFile.EndsWith(".ccd", StringComparison.OrdinalIgnoreCase))
                    {
                        string isoFile = Path.Combine(
                            Path.GetDirectoryName(inputFile) ?? string.Empty,
                            Path.GetFileNameWithoutExtension(inputFile) + ".iso"
                        );
                        if (File.Exists(isoFile))
                        {
                            File.Delete(isoFile);
                        }
                    }
                }
            });
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (isConverting)
            {
                cancellationTokenSource?.Cancel();
                Button btnStop = (Button)sender;
                btnStop.Enabled = false;
                
                if (currentProcess != null && !currentProcess.HasExited)
                {
                    try
                    {
                        currentProcess.Kill();
                    }
                    catch { }
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Очищаем временную папку
            try
            {
                if (Directory.Exists(settings.TempFolder))
                {
                    Directory.Delete(settings.TempFolder, true);
                }
            }
            catch { }
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            var t = translations.ContainsKey(settings.Language) ? translations[settings.Language] : translations["ru"];
            using (Form aboutForm = new Form())
            {
                aboutForm.Text = t["AboutTitle"];
                aboutForm.Size = new System.Drawing.Size(600, 500);
                aboutForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                aboutForm.MaximizeBox = false;
                aboutForm.MinimizeBox = false;
                aboutForm.StartPosition = FormStartPosition.CenterParent;
                aboutForm.Icon = this.Icon;

                RichTextBox textBox = new RichTextBox
                {
                    Location = new System.Drawing.Point(10, 10),
                    Size = new System.Drawing.Size(565, 440),
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = SystemColors.Control
                };

                textBox.Text = $@"GUI2CHD - Конвертер образов в CHD

{t["AboutVersion"]}

{t["AboutAuthor"]}

{t["AboutDisclaimer"]}

{t["AboutLicense"]}

{t["MITLicense"]}

{t["AboutComponents"]}

{t["AboutComponent1"]}

{t["AboutComponent2"]}

{t["AboutComponent3"]}

{t["AboutComponent4"]}

{t["AboutLegalNotice"]}";

                Button btnOK = new Button
                {
                    Text = t["OK"],
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(500, 460),
                    Size = new System.Drawing.Size(75, 23)
                };

                aboutForm.Controls.AddRange(new Control[] { textBox, btnOK });
                aboutForm.AcceptButton = btnOK;

                aboutForm.ShowDialog();
            }
        }

        private void PendingList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                bool hasValidFiles = files.Any(f => 
                {
                    string ext = Path.GetExtension(f).ToLower();
                    return ext == ".iso" || ext == ".cue" || ext == ".ccd" || 
                           ext == ".gdi" || ext == ".7z" || ext == ".zip" || ext == ".rar";
                });

                if (hasValidFiles)
                {
                    e.Effect = DragDropEffects.Copy;
                    pendingList.BackColor = System.Drawing.Color.LightGreen;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    pendingList.BackColor = System.Drawing.Color.LightPink;
                }
            }
        }

        private void PendingList_DragLeave(object sender, EventArgs e)
        {
            pendingList.BackColor = System.Drawing.SystemColors.Window;
        }

        private void PendingList_DragDrop(object sender, DragEventArgs e)
        {
            pendingList.BackColor = System.Drawing.SystemColors.Window;
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                try
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    ProcessFiles(files);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обработке перетаскиваемых файлов: {ex.Message}", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PendingList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && pendingList.SelectedItems.Count > 0)
            {
                foreach (string item in pendingList.SelectedItems)
                {
                    pendingFiles.Remove(item);
                }
                UpdateFileLists();
            }
        }

        private void LanguageMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string lang)
            {
                settings.Language = lang;
                settings.Save();
                ApplyLocalization();
            }
        }

        private void ApplyLocalization()
        {
            var lang = settings.Language;
            var t = translations.ContainsKey(lang) ? translations[lang] : translations["ru"];

            // Меню
            var menuStrip = this.MainMenuStrip;
            if (menuStrip != null)
            {
                foreach (ToolStripMenuItem item in menuStrip.Items)
                {
                    if (item.Text == "Настройки" || item.Text == "Settings" || item.Text == "设置")
                        item.Text = t["MenuSettings"];
                    else if (item.Text == "Справка" || item.Text == "Help" || item.Text == "帮助")
                        item.Text = t["MenuHelp"];
                    else if (item.Text == "Язык" || item.Text == "Language" || item.Text == "语言")
                        item.Text = t["MenuLanguage"];
                }
                // Подменю
                foreach (ToolStripMenuItem item in menuStrip.Items)
                {
                    foreach (ToolStripItem sub in item.DropDownItems)
                    {
                        if (sub.Text == "Пути" || sub.Text == "Paths" || sub.Text == "路径")
                            sub.Text = t["MenuPaths"];
                        else if (sub.Text == "О программе" || sub.Text == "About" || sub.Text == "关于")
                            sub.Text = t["MenuAbout"];
                        else if (sub.Text == "Русский" || sub.Text == "English" || sub.Text == "简体中文")
                        {
                            if (sub is ToolStripMenuItem mi && mi.Tag is string tag)
                                sub.Text = t[$"Lang{tag.Substring(0,1).ToUpper()}{tag.Substring(1)}"];
                        }
                    }
                }
            }
            // Кнопки
            foreach (Button btn in this.Controls.OfType<Button>())
            {
                if (btn.Text == "Выбрать файлы" || btn.Text == "Select Files" || btn.Text == "选择文件")
                    btn.Text = t["SelectFiles"];
                else if (btn.Text == "Конвертировать" || btn.Text == "Convert" || btn.Text == "转换")
                    btn.Text = t["Convert"];
                else if (btn.Text == "Остановить" || btn.Text == "Stop" || btn.Text == "停止")
                    btn.Text = t["Stop"];
            }
            // Метки
            if (lblPending != null) lblPending.Text = t["Pending"];
            if (lblCompletedList != null) lblCompletedList.Text = t["Completed"];
            if (lblMainLog != null) lblMainLog.Text = t["MainLog"];
            lblDetailedLog = this.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblDetailedLog");
            if (lblDetailedLog != null) lblDetailedLog.Text = t["DetailedLog"];
            if (lblOutputFolder != null && (lblOutputFolder.Text == "Папка не выбрана" || lblOutputFolder.Text == "Folder not selected" || lblOutputFolder.Text == "未选择文件夹"))
                lblOutputFolder.Text = t["OutputFolder"];
            if (lblExtractProgress != null) lblExtractProgress.Text = t["Progress"];
        }
    }
} 