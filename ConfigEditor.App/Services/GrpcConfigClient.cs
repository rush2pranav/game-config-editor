using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ConfigEditor.Shared.Protos;

namespace ConfigEditor.App.Services
{
    public class GrpcConfigClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly ConfigService.ConfigServiceClient _client;

        public GrpcConfigClient(string serverUrl = "http://localhost:5080")
        {
            _channel = GrpcChannel.ForAddress(serverUrl);
            _client = new ConfigService.ConfigServiceClient(_channel);
        }

        // for weapons
        public async Task<List<WeaponReply>> GetWeaponsAsync()
        {
            var reply = await _client.GetWeaponsAsync(new GetAllRequest());
            return reply.Weapons.ToList();
        }

        public async Task<SaveReply> SaveWeaponAsync(WeaponReply weapon)
            => await _client.SaveWeaponAsync(weapon);

        public async Task<SaveReply> DeleteWeaponAsync(int id)
            => await _client.DeleteWeaponAsync(new GetByIdRequest { Id = id });

        // for enemies
        public async Task<List<EnemyReply>> GetEnemiesAsync()
        {
            var reply = await _client.GetEnemiesAsync(new GetAllRequest());
            return reply.Enemies.ToList();
        }

        public async Task<SaveReply> SaveEnemyAsync(EnemyReply enemy)
            => await _client.SaveEnemyAsync(enemy);

        public async Task<SaveReply> DeleteEnemyAsync(int id)
            => await _client.DeleteEnemyAsync(new GetByIdRequest { Id = id });

        // for items
        public async Task<List<ItemReply>> GetItemsAsync()
        {
            var reply = await _client.GetItemsAsync(new GetAllRequest());
            return reply.Items.ToList();
        }

        public async Task<SaveReply> SaveItemAsync(ItemReply item)
            => await _client.SaveItemAsync(item);

        public async Task<SaveReply> DeleteItemAsync(int id)
            => await _client.DeleteItemAsync(new GetByIdRequest { Id = id });

        // for health
        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                await _client.GetWeaponsAsync(new GetAllRequest());
                return true;
            }
            catch { return false; }
        }

        public void Dispose() => _channel.Dispose();
    }
}
