using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ConfigEditor.App.Commands;
using ConfigEditor.App.Services;
using ConfigEditor.Shared.Protos;

namespace ConfigEditor.App.ViewModels
{
    public class EditorViewModel : INotifyPropertyChanged
    {
        private readonly GrpcConfigClient _client;
        private readonly UndoRedoService _undoRedo;

        public EditorViewModel()
        {
            _client = new GrpcConfigClient();
            _undoRedo = new UndoRedoService();
            _undoRedo.StateChanged += () =>
            {
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(CanRedo));
                OnPropertyChanged(nameof(UndoTooltip));
                OnPropertyChanged(nameof(RedoTooltip));
            };

            Weapons = new ObservableCollection<WeaponReply>();
            Enemies = new ObservableCollection<EnemyReply>();
            Items = new ObservableCollection<ItemReply>();
            Categories = new ObservableCollection<string> { "Rifle", "Pistol", "Shotgun", "SMG", "Sniper", "Heavy", "Melee" };
            Rarities = new ObservableCollection<string> { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
            EnemyTypes = new ObservableCollection<string> { "Standard", "Elite", "Boss", "Minion" };
            ItemTypes = new ObservableCollection<string> { "Weapon", "Armor", "Consumable", "Material", "Quest" };

            // commands
            SaveCommand = new RelayCommand(async _ => await SaveCurrentAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteCurrentAsync(), _ => SelectedWeapon != null || SelectedEnemy != null || SelectedItem != null);
            NewWeaponCommand = new RelayCommand(_ => CreateNewWeapon());
            NewEnemyCommand = new RelayCommand(_ => CreateNewEnemy());
            NewItemCommand = new RelayCommand(_ => CreateNewItem());
            RefreshCommand = new RelayCommand(async _ => await LoadAllAsync());
            UndoCommand = new RelayCommand(_ => _undoRedo.Undo(), _ => CanUndo);
            RedoCommand = new RelayCommand(_ => _undoRedo.Redo(), _ => CanRedo);

            _ = InitializeAsync();
        }

        // commands
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand NewWeaponCommand { get; }
        public ICommand NewEnemyCommand { get; }
        public ICommand NewItemCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        // collections
        public ObservableCollection<WeaponReply> Weapons { get; }
        public ObservableCollection<EnemyReply> Enemies { get; }
        public ObservableCollection<ItemReply> Items { get; }
        public ObservableCollection<string> Categories { get; }
        public ObservableCollection<string> Rarities { get; }
        public ObservableCollection<string> EnemyTypes { get; }
        public ObservableCollection<string> ItemTypes { get; }

        // for selected tab
        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
        }

        // for selected items
        private WeaponReply? _selectedWeapon;
        public WeaponReply? SelectedWeapon
        {
            get => _selectedWeapon;
            set
            {
                _selectedWeapon = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasWeaponSelected));
                if (value != null) LoadWeaponToEditor(value);
            }
        }

        private EnemyReply? _selectedEnemy;
        public EnemyReply? SelectedEnemy
        {
            get => _selectedEnemy;
            set
            {
                _selectedEnemy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasEnemySelected));
                if (value != null) LoadEnemyToEditor(value);
            }
        }

        private ItemReply? _selectedItem;
        public ItemReply? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasItemSelected));
                if (value != null) LoadItemToEditor(value);
            }
        }

        public bool HasWeaponSelected => SelectedWeapon != null;
        public bool HasEnemySelected => SelectedEnemy != null;
        public bool HasItemSelected => SelectedItem != null;

        // weapon editor fields
        private string _wName = ""; public string WName { get => _wName; set { _wName = value; OnPropertyChanged(); } }
        private string _wCategory = "Rifle"; public string WCategory { get => _wCategory; set { _wCategory = value; OnPropertyChanged(); } }
        private int _wDamage; public int WDamage { get => _wDamage; set { _wDamage = value; OnPropertyChanged(); } }
        private double _wFireRate; public double WFireRate { get => _wFireRate; set { _wFireRate = value; OnPropertyChanged(); } }
        private int _wMagSize; public int WMagSize { get => _wMagSize; set { _wMagSize = value; OnPropertyChanged(); } }
        private double _wReloadTime; public double WReloadTime { get => _wReloadTime; set { _wReloadTime = value; OnPropertyChanged(); } }
        private double _wAccuracy; public double WAccuracy { get => _wAccuracy; set { _wAccuracy = value; OnPropertyChanged(); } }
        private double _wRange; public double WRange { get => _wRange; set { _wRange = value; OnPropertyChanged(); } }
        private int _wCost; public int WCost { get => _wCost; set { _wCost = value; OnPropertyChanged(); } }
        private string _wRarity = "Common"; public string WRarity { get => _wRarity; set { _wRarity = value; OnPropertyChanged(); } }
        private bool _wEnabled = true; public bool WEnabled { get => _wEnabled; set { _wEnabled = value; OnPropertyChanged(); } }
        private string _wDesc = ""; public string WDesc { get => _wDesc; set { _wDesc = value; OnPropertyChanged(); } }

        // enemy editor field section
        private string _eName = ""; public string EName { get => _eName; set { _eName = value; OnPropertyChanged(); } }
        private string _eType = "Standard"; public string EType { get => _eType; set { _eType = value; OnPropertyChanged(); } }
        private int _eHealth; public int EHealth { get => _eHealth; set { _eHealth = value; OnPropertyChanged(); } }
        private int _eDamage; public int EDamage { get => _eDamage; set { _eDamage = value; OnPropertyChanged(); } }
        private double _eMoveSpeed; public double EMoveSpeed { get => _eMoveSpeed; set { _eMoveSpeed = value; OnPropertyChanged(); } }
        private int _eXpReward; public int EXpReward { get => _eXpReward; set { _eXpReward = value; OnPropertyChanged(); } }
        private double _eSpawnChance; public double ESpawnChance { get => _eSpawnChance; set { _eSpawnChance = value; OnPropertyChanged(); } }
        private int _eMinLevel = 1; public int EMinLevel { get => _eMinLevel; set { _eMinLevel = value; OnPropertyChanged(); } }
        private string _eLootTable = ""; public string ELootTable { get => _eLootTable; set { _eLootTable = value; OnPropertyChanged(); } }
        private bool _eEnabled = true; public bool EEnabled { get => _eEnabled; set { _eEnabled = value; OnPropertyChanged(); } }
        private string _eDesc = ""; public string EDesc { get => _eDesc; set { _eDesc = value; OnPropertyChanged(); } }

        // item editor fields
        private string _iName = ""; public string IName { get => _iName; set { _iName = value; OnPropertyChanged(); } }
        private string _iType = "Consumable"; public string IType { get => _iType; set { _iType = value; OnPropertyChanged(); } }
        private string _iRarity = "Common"; public string IRarity { get => _iRarity; set { _iRarity = value; OnPropertyChanged(); } }
        private int _iValue; public int IValue { get => _iValue; set { _iValue = value; OnPropertyChanged(); } }
        private double _iDropRate; public double IDropRate { get => _iDropRate; set { _iDropRate = value; OnPropertyChanged(); } }
        private int _iMaxStack = 1; public int IMaxStack { get => _iMaxStack; set { _iMaxStack = value; OnPropertyChanged(); } }
        private int _iLevelReq = 1; public int ILevelReq { get => _iLevelReq; set { _iLevelReq = value; OnPropertyChanged(); } }
        private bool _iTradeable = true; public bool ITradeable { get => _iTradeable; set { _iTradeable = value; OnPropertyChanged(); } }
        private bool _iEnabled = true; public bool IEnabled { get => _iEnabled; set { _iEnabled = value; OnPropertyChanged(); } }
        private string _iDesc = ""; public string IDesc { get => _iDesc; set { _iDesc = value; OnPropertyChanged(); } }

        // undo/redo section
        public bool CanUndo => _undoRedo.CanUndo;
        public bool CanRedo => _undoRedo.CanRedo;
        public string UndoTooltip => _undoRedo.LastUndoDescription != null ? $"Undo: {_undoRedo.LastUndoDescription}" : "Nothing to undo";
        public string RedoTooltip => _undoRedo.LastRedoDescription != null ? $"Redo: {_undoRedo.LastRedoDescription}" : "Nothing to redo";

        // status section
        private string _statusText = "Connecting...";
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

        private bool _isConnected;
        public bool IsConnected { get => _isConnected; set { _isConnected = value; OnPropertyChanged(); } }

        // methods section
        private async Task InitializeAsync()
        {
            IsConnected = await _client.IsConnectedAsync();
            if (IsConnected)
            {
                await LoadAllAsync();
                StatusText = "Connected to gRPC server";
            }
            else
            {
                StatusText = "Cannot connect. Start ConfigEditor.Server first (localhost:5080)";
            }
        }

        private async Task LoadAllAsync()
        {
            try
            {
                var weapons = await _client.GetWeaponsAsync();
                Weapons.Clear();
                foreach (var w in weapons) Weapons.Add(w);

                var enemies = await _client.GetEnemiesAsync();
                Enemies.Clear();
                foreach (var e in enemies) Enemies.Add(e);

                var items = await _client.GetItemsAsync();
                Items.Clear();
                foreach (var i in items) Items.Add(i);

                StatusText = $"Loaded {Weapons.Count} weapons, {Enemies.Count} enemies, {Items.Count} items";
            }
            catch (Exception ex)
            {
                StatusText = $"Error loading data: {ex.Message}";
            }
        }

        private void LoadWeaponToEditor(WeaponReply w)
        {
            WName = w.Name; WCategory = w.Category; WDamage = w.Damage;
            WFireRate = w.FireRate; WMagSize = w.MagazineSize; WReloadTime = w.ReloadTime;
            WAccuracy = w.Accuracy; WRange = w.Range; WCost = w.Cost;
            WRarity = w.Rarity; WEnabled = w.IsEnabled; WDesc = w.Description;
        }

        private void LoadEnemyToEditor(EnemyReply e)
        {
            EName = e.Name; EType = e.EnemyType; EHealth = e.Health;
            EDamage = e.Damage; EMoveSpeed = e.MoveSpeed; EXpReward = e.XpReward;
            ESpawnChance = e.SpawnChance; EMinLevel = e.MinLevel;
            ELootTable = e.LootTable; EEnabled = e.IsEnabled; EDesc = e.Description;
        }

        private void LoadItemToEditor(ItemReply i)
        {
            IName = i.Name; IType = i.ItemType; IRarity = i.Rarity;
            IValue = i.Value; IDropRate = i.DropRate; IMaxStack = i.MaxStack;
            ILevelReq = i.LevelRequired; ITradeable = i.IsTradeable;
            IEnabled = i.IsEnabled; IDesc = i.Description;
        }

        private async Task SaveCurrentAsync()
        {
            try
            {
                if (SelectedTab == 0) await SaveWeaponAsync();
                else if (SelectedTab == 1) await SaveEnemyAsync();
                else if (SelectedTab == 2) await SaveItemAsync();
            }
            catch (Exception ex)
            {
                StatusText = $"Save failed: {ex.Message}";
            }
        }

        private async Task SaveWeaponAsync()
        {
            if (string.IsNullOrWhiteSpace(WName)) { StatusText = "Weapon name is required"; return; }

            var weapon = new WeaponReply
            {
                Id = SelectedWeapon?.Id ?? 0,
                Name = WName,
                Category = WCategory,
                Damage = WDamage,
                FireRate = WFireRate,
                MagazineSize = WMagSize,
                ReloadTime = WReloadTime,
                Accuracy = WAccuracy,
                Range = WRange,
                Cost = WCost,
                Rarity = WRarity,
                IsEnabled = WEnabled,
                Description = WDesc ?? ""
            };

            // capturing the old state for undo
            var oldWeapon = SelectedWeapon;
            var isNew = weapon.Id == 0;

            var reply = await _client.SaveWeaponAsync(weapon);
            if (reply.Success)
            {
                _undoRedo.Execute(new UndoableAction(
                    isNew ? $"Create weapon '{WName}'" : $"Edit weapon '{WName}'",
                    () => { }, // already executed
                    async () =>
                    {
                        if (isNew) await _client.DeleteWeaponAsync(reply.Id);
                        else if (oldWeapon != null) await _client.SaveWeaponAsync(oldWeapon);
                        Application.Current.Dispatcher.Invoke(async () => await LoadAllAsync());
                    }
                ));
                await LoadAllAsync();
                StatusText = $"Saved weapon: {WName}";
            }
            else StatusText = $"Failed: {reply.Message}";
        }

        private async Task SaveEnemyAsync()
        {
            if (string.IsNullOrWhiteSpace(EName)) { StatusText = "Enemy name is required"; return; }

            var enemy = new EnemyReply
            {
                Id = SelectedEnemy?.Id ?? 0,
                Name = EName,
                EnemyType = EType,
                Health = EHealth,
                Damage = EDamage,
                MoveSpeed = EMoveSpeed,
                XpReward = EXpReward,
                SpawnChance = ESpawnChance,
                MinLevel = EMinLevel,
                LootTable = ELootTable ?? "",
                IsEnabled = EEnabled,
                Description = EDesc ?? ""
            };

            var reply = await _client.SaveEnemyAsync(enemy);
            if (reply.Success) { await LoadAllAsync(); StatusText = $"Saved enemy: {EName}"; }
            else StatusText = $"Failed: {reply.Message}";
        }

        private async Task SaveItemAsync()
        {
            if (string.IsNullOrWhiteSpace(IName)) { StatusText = "Item name is required"; return; }

            var item = new ItemReply
            {
                Id = SelectedItem?.Id ?? 0,
                Name = IName,
                ItemType = IType,
                Rarity = IRarity,
                Value = IValue,
                DropRate = IDropRate,
                MaxStack = IMaxStack,
                LevelRequired = ILevelReq,
                IsTradeable = ITradeable,
                IsEnabled = IEnabled,
                Description = IDesc ?? ""
            };

            var reply = await _client.SaveItemAsync(item);
            if (reply.Success) { await LoadAllAsync(); StatusText = $"Saved item: {IName}"; }
            else StatusText = $"Failed: {reply.Message}";
        }

        private async Task DeleteCurrentAsync()
        {
            SaveReply? reply = null;
            if (SelectedTab == 0 && SelectedWeapon != null)
            {
                reply = await _client.DeleteWeaponAsync(SelectedWeapon.Id);
                if (reply.Success) StatusText = $"Deleted weapon: {SelectedWeapon.Name}";
            }
            else if (SelectedTab == 1 && SelectedEnemy != null)
            {
                reply = await _client.DeleteEnemyAsync(SelectedEnemy.Id);
                if (reply.Success) StatusText = $"Deleted enemy: {SelectedEnemy.Name}";
            }
            else if (SelectedTab == 2 && SelectedItem != null)
            {
                reply = await _client.DeleteItemAsync(SelectedItem.Id);
                if (reply.Success) StatusText = $"Deleted item: {SelectedItem.Name}";
            }

            if (reply?.Success == true) await LoadAllAsync();
        }

        private void CreateNewWeapon()
        {
            SelectedWeapon = null;
            WName = "New Weapon"; WCategory = "Rifle"; WDamage = 30; WFireRate = 5;
            WMagSize = 30; WReloadTime = 2; WAccuracy = 70; WRange = 40;
            WCost = 1000; WRarity = "Common"; WEnabled = true; WDesc = "";
            StatusText = "Creating new weapon - fill in details and click Save";
        }

        private void CreateNewEnemy()
        {
            SelectedEnemy = null;
            EName = "New Enemy"; EType = "Standard"; EHealth = 100; EDamage = 20;
            EMoveSpeed = 3; EXpReward = 50; ESpawnChance = 20; EMinLevel = 1;
            ELootTable = "common_loot"; EEnabled = true; EDesc = "";
            StatusText = "Creating new enemy - fill in details and click Save";
        }

        private void CreateNewItem()
        {
            SelectedItem = null;
            IName = "New Item"; IType = "Consumable"; IRarity = "Common";
            IValue = 100; IDropRate = 10; IMaxStack = 99; ILevelReq = 1;
            ITradeable = true; IEnabled = true; IDesc = "";
            StatusText = "Creating new item — fill in details and click Save";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    }
}
