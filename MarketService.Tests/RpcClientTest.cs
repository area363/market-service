using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Bencodex;
using Bencodex.Types;
using Grpc.Core;
using Lib9c.Model.Order;
using Libplanet;
using Libplanet.Action;
using Libplanet.Assets;
using Libplanet.Crypto;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.Shared.Services;
using Nekoyume.TableData;
using Nekoyume.TableData.Crystal;
using Xunit;

namespace MarketService.Tests;

public class RpcClientTest
{
    private readonly CrystalEquipmentGrindingSheet _crystalEquipmentGrindingSheet;
    private readonly CrystalMonsterCollectionMultiplierSheet _crystalMonsterCollectionMultiplierSheet;
    private readonly EquipmentItemSheet.Row _row;
    private readonly TestService _testService;

    public RpcClientTest()
    {
        _testService = new TestService();
        _row = new EquipmentItemSheet.Row();
        _row.Set(@"10200000,Armor,0,Normal,0,HP,30,2,Character/Player/10200000".Split(","));
        _crystalEquipmentGrindingSheet = new CrystalEquipmentGrindingSheet();
        _crystalEquipmentGrindingSheet.Set(@"id,enchant_base_id,gain_crystal
10100000,10100000,10
10110000,10110000,10
10111000,10110000,19
10112000,10110000,19
10113000,10110000,184
10114000,10110000,636
10120000,10120000,80
10121000,10120000,101
10122000,10120000,144
10123000,10120000,1056
10124000,10120000,11280
10130000,10130000,2340
10131000,10130000,2784
10132000,10130000,3168
10133000,10130000,20520
10134000,10130000,55680
10130001,10130001,8340
10131001,10130001,14520
10132001,10130001,16020
10133001,10130001,72060
10134001,10130001,165300
10140000,10141000,2000000
10141000,10141000,1000000
10142000,10141000,1000000
10143000,10141000,1500000
10144000,10141000,1500000
10140001,10140001,306600
10141001,10140001,329640
10142001,10140001,332820
10143001,10140001,332820
10144001,10140001,337380
10150000,10150000,1008600
10151000,10150000,1046220
10152000,10150000,1056780
10153000,10150000,1065960
10154000,10150000,1076520
10150001,10150001,1123800
10151001,10150001,1164660
10152001,10150001,1175220
10153001,10150001,1194960
10154001,10150001,1204140
10155000,10155000,1200600
10200000,10200000,10
10210000,10210000,10
10211000,10210000,19
10212000,10210000,27
10213000,10210000,230
10214000,10210000,742
10220000,10220000,50
10221000,10220000,74
10222000,10220000,103
10223000,10220000,928
10224000,10220000,11960
10230000,10230000,3000
10231000,10230000,3480
10232000,10230000,3864
10233000,10230000,46800
10234000,10230000,60840
10230001,10230001,9600
10231001,10230001,16500
10232001,10230001,17520
10233001,10230001,151800
10234001,10230001,296400
10240000,10241000,2000000
10241000,10241000,1000000
10242000,10241000,1000000
10243000,10241000,1500000
10244000,10241000,1500000
10240001,10240001,260760
10241001,10240001,282900
10242001,10240001,286620
10243001,10240001,286620
10244001,10240001,289800
10250000,10250000,807840
10251000,10250000,831600
10252000,10250000,841320
10253000,10250000,850500
10254000,10250000,888300
10250001,10250001,1019160
10251001,10250001,1058400
10252001,10250001,1068120
10253001,10250001,1125180
10254001,10250001,1135740
10255000,10255000,1129320
10310000,10310000,10
10311000,10310000,19
10312000,10310000,41
10313000,10310000,252
10314000,10310000,954
10320000,10320000,120
10321000,10320000,144
10322000,10320000,216
10323000,10320000,1496
10324000,10320000,40740
10330000,10330000,3300
10331000,10330000,3864
10332000,10330000,4872
10333000,10330000,67860
10334000,10330000,247420
10340000,10340000,24948
10341000,10340000,28980
10342000,10340000,376200
10343000,10340000,376200
10344000,10340000,376200
10350000,10351000,2000000
10351000,10351000,1000000
10352000,10351000,1000000
10353000,10351000,1500000
10354000,10351000,1500000
10410000,10410000,10
10411000,10410000,19
10412000,10410000,41
10413000,10410000,315
10414000,10410000,1060
10420000,10420000,140
10421000,10420000,193
10422000,10420000,245
10423000,10420000,1736
10424000,10420000,38960
10430000,10430000,4320
10431000,10430000,4560
10432000,10430000,5952
10433000,10430000,65040
10434000,10430000,157500
10440000,10440000,243000
10441000,10440000,509400
10442000,10440000,509400
10443000,10440000,509400
10444000,10440000,509400
10450000,10451000,2000000
10451000,10451000,1000000
10452000,10451000,1000000
10453000,10451000,1500000
10454000,10451000,1500000
10510000,10510000,10
10511000,10510000,19
10512000,10510000,46
10513000,10510000,373
10514000,10510000,1272
10520000,10520000,160
10521000,10520000,216
10522000,10520000,288
10523000,10520000,1904
10524000,10520000,44300
10530000,10530000,4980
10531000,10530000,5568
10532000,10530000,6648
10533000,10530000,249600
10534000,10530000,296400
10540000,10541000,2000000
10541000,10541000,1000000
10542000,10541000,1000000
10543000,10541000,1500000
10544000,10541000,1500000
10550000,10550000,918000
10551000,10550000,954720
10552000,10550000,963900
10553000,10550000,973620
10554000,10550000,1026480
11320000,10320000,120
11420000,10420000,140
11520000,10520000,160");
        _crystalMonsterCollectionMultiplierSheet = new CrystalMonsterCollectionMultiplierSheet();
        _crystalMonsterCollectionMultiplierSheet.Set(@"level,multiplier
0,0
1,0
2,50
3,100
4,200
5,300");
    }

    [Fact]
    public async Task SyncOrder_Cancel()
    {
        var ct = new CancellationToken();
        var receiver = new Receiver(new Logger<Receiver>(new LoggerFactory()));
#pragma warning disable EF1001
        var contextFactory = new DbContextFactory<MarketContext>(null!,
            new DbContextOptionsBuilder<MarketContext>().UseNpgsql(@"Host=localhost;Username=postgres;Database=test")
                .UseLowerCaseNamingConvention().Options, new DbContextFactorySource<MarketContext>());
        var context = await contextFactory.CreateDbContextAsync(ct);
#pragma warning restore EF1001
        await context.Database.EnsureDeletedAsync(ct);
        await context.Database.EnsureCreatedAsync(ct);
        var rpcConfigOptions = new RpcConfigOptions {Host = "localhost", Port = 5000};
        var client = new TestClient(new OptionsWrapper<RpcConfigOptions>(rpcConfigOptions),
            new Logger<RpcClient>(new LoggerFactory()), receiver, contextFactory, _testService);
        var agentAddress = new PrivateKey().ToAddress();
        var avatarAddress = new PrivateKey().ToAddress();
#pragma warning disable CS0618
        var currency = Currency.Legacy("NCG", 2, null);
#pragma warning restore CS0618
        var order = OrderFactory.Create(agentAddress, avatarAddress, Guid.NewGuid(), 1 * currency, Guid.NewGuid(), 0L,
            ItemSubType.Armor, 1);
        _testService.SetOrder(order);
        var shopAddress = ShardedShopStateV2.DeriveAddress(ItemSubType.Armor, order.OrderId);
        var shopState = new ShardedShopStateV2(shopAddress);
        var item = ItemFactory.CreateItemUsable(_row, order.TradableId, 0L);
        var orderDigest = new OrderDigest(
            agentAddress,
            0L,
            Order.ExpirationInterval,
            order.OrderId,
            order.TradableId,
            order.Price,
            0,
            0,
            item.Id,
            1
        );
        shopState.Add(orderDigest, 0L);
        _testService.SetState(shopAddress, shopState.Serialize());
        _testService.SetState(Addresses.GetItemAddress(item.TradableId), item.Serialize());

        // Insert order
        await client.SyncOrder(ItemSubType.Armor, null!, _crystalEquipmentGrindingSheet,
            _crystalMonsterCollectionMultiplierSheet);
        var productModel = Assert.Single(context.Products);
        Assert.True(productModel.Legacy);
        Assert.True(productModel.Exist);

        // Cancel order
        shopState.Remove(order, 1L);
        _testService.SetState(shopAddress, shopState.Serialize());
        await client.SyncOrder(ItemSubType.Armor, null!, _crystalEquipmentGrindingSheet,
            _crystalMonsterCollectionMultiplierSheet);
#pragma warning disable EF1001
        var nextContext = await contextFactory.CreateDbContextAsync(ct);
#pragma warning restore EF1001
        var updatedProductModel = Assert.Single(nextContext.Products);
        Assert.Equal(productModel.ProductId, updatedProductModel.ProductId);
        Assert.False(updatedProductModel.Exist);

        // Buy order
    }

    [Fact]
    public async Task SyncOrder_ReRegister()
    {
        var ct = new CancellationToken();
        var receiver = new Receiver(new Logger<Receiver>(new LoggerFactory()));
#pragma warning disable EF1001
        var contextFactory = new DbContextFactory<MarketContext>(null!,
            new DbContextOptionsBuilder<MarketContext>().UseNpgsql(@"Host=localhost;Username=postgres;Database=test")
                .UseLowerCaseNamingConvention().Options, new DbContextFactorySource<MarketContext>());
        var context = await contextFactory.CreateDbContextAsync(ct);
#pragma warning restore EF1001
        await context.Database.EnsureDeletedAsync(ct);
        await context.Database.EnsureCreatedAsync(ct);
        var rpcConfigOptions = new RpcConfigOptions {Host = "localhost", Port = 5000};
        var client = new TestClient(new OptionsWrapper<RpcConfigOptions>(rpcConfigOptions),
            new Logger<RpcClient>(new LoggerFactory()), receiver, contextFactory, _testService);
        var agentAddress = new PrivateKey().ToAddress();
        var avatarAddress = new PrivateKey().ToAddress();
#pragma warning disable CS0618
        var currency = Currency.Legacy("NCG", 2, null);
#pragma warning restore CS0618
        var order = OrderFactory.Create(agentAddress, avatarAddress, Guid.NewGuid(), 1 * currency, Guid.NewGuid(), 0L,
            ItemSubType.Armor, 1);
        _testService.SetOrder(order);
        var shopAddress = ShardedShopStateV2.DeriveAddress(ItemSubType.Armor, order.OrderId);
        var shopState = new ShardedShopStateV2(shopAddress);
        var item = ItemFactory.CreateItemUsable(_row, order.TradableId, 0L);
        var orderDigest = new OrderDigest(
            agentAddress,
            0L,
            Order.ExpirationInterval,
            order.OrderId,
            order.TradableId,
            order.Price,
            0,
            0,
            item.Id,
            1
        );
        shopState.Add(orderDigest, 0L);
        _testService.SetState(shopAddress, shopState.Serialize());
        _testService.SetState(Addresses.GetItemAddress(item.TradableId), item.Serialize());

        // Insert order
        await client.SyncOrder(ItemSubType.Armor, null!, _crystalEquipmentGrindingSheet,
            _crystalMonsterCollectionMultiplierSheet);
        var productModel = Assert.Single(context.Products);
        Assert.True(productModel.Legacy);
        Assert.True(productModel.Exist);

        // ReRegister order
        shopState.Remove(order, 1L);
        _testService.SetState(shopAddress, shopState.Serialize());

        var order2 = OrderFactory.Create(agentAddress, avatarAddress, Guid.NewGuid(), 2 * currency, order.TradableId,
            1L,
            ItemSubType.Armor, 1);
        var shopAddress2 = ShardedShopStateV2.DeriveAddress(ItemSubType.Armor, order2.OrderId);
        var shopState2 = new ShardedShopStateV2(shopAddress2);
        var orderDigest2 = new OrderDigest(
            agentAddress,
            1L,
            Order.ExpirationInterval + 1L,
            order2.OrderId,
            order2.TradableId,
            order2.Price,
            0,
            0,
            item.Id,
            1
        );
        shopState2.Add(orderDigest2, 1L);
        _testService.SetState(Order.DeriveAddress(order2.OrderId), order2.Serialize());
        _testService.SetState(shopAddress2, shopState2.Serialize());

        await client.SyncOrder(ItemSubType.Armor, null!, _crystalEquipmentGrindingSheet,
            _crystalMonsterCollectionMultiplierSheet);
#pragma warning disable EF1001
        var nextContext = await contextFactory.CreateDbContextAsync(ct);
#pragma warning restore EF1001
        Assert.Equal(2, nextContext.Products.Count());
        var oldProduct = nextContext.Products.Single(p => p.ProductId == order.OrderId);
        Assert.Equal(1, oldProduct.Price);
        Assert.False(oldProduct.Exist);
        var newProduct = nextContext.Products.Single(p => p.ProductId == order2.OrderId);
        Assert.Equal(2, newProduct.Price);
        Assert.True(newProduct.Exist);

        // Buy order
    }

    [Fact]
    public async Task SyncOrder_Buy()
    {
        var ct = new CancellationToken();
        var receiver = new Receiver(new Logger<Receiver>(new LoggerFactory()));
#pragma warning disable EF1001
        var contextFactory = new DbContextFactory<MarketContext>(null!,
            new DbContextOptionsBuilder<MarketContext>().UseNpgsql(@"Host=localhost;Username=postgres;Database=test")
                .UseLowerCaseNamingConvention().Options, new DbContextFactorySource<MarketContext>());
        var context = await contextFactory.CreateDbContextAsync(ct);
#pragma warning restore EF1001
        await context.Database.EnsureDeletedAsync(ct);
        await context.Database.EnsureCreatedAsync(ct);
        var rpcConfigOptions = new RpcConfigOptions {Host = "localhost", Port = 5000};
        var client = new TestClient(new OptionsWrapper<RpcConfigOptions>(rpcConfigOptions),
            new Logger<RpcClient>(new LoggerFactory()), receiver, contextFactory, _testService);
        var agentAddress = new PrivateKey().ToAddress();
        var avatarAddress = new PrivateKey().ToAddress();
#pragma warning disable CS0618
        var currency = Currency.Legacy("NCG", 2, null);
#pragma warning restore CS0618
        var order = OrderFactory.Create(agentAddress, avatarAddress, Guid.NewGuid(), 1 * currency, Guid.NewGuid(), 0L,
            ItemSubType.Armor, 1);
        _testService.SetOrder(order);
        var shopAddress = ShardedShopStateV2.DeriveAddress(ItemSubType.Armor, order.OrderId);
        var shopState = new ShardedShopStateV2(shopAddress);
        var item = ItemFactory.CreateItemUsable(_row, order.TradableId, 0L);
        var orderDigest = new OrderDigest(
            agentAddress,
            0L,
            Order.ExpirationInterval,
            order.OrderId,
            order.TradableId,
            order.Price,
            0,
            0,
            item.Id,
            1
        );
        shopState.Add(orderDigest, 0L);
        _testService.SetState(shopAddress, shopState.Serialize());
        _testService.SetState(Addresses.GetItemAddress(item.TradableId), item.Serialize());

        // Insert order
        await client.SyncOrder(ItemSubType.Armor, null!, _crystalEquipmentGrindingSheet,
            _crystalMonsterCollectionMultiplierSheet);
        var productModel = Assert.Single(context.Products);
        Assert.True(productModel.Legacy);
        Assert.True(productModel.Exist);

        // ReRegister order
        shopState.Remove(order, 1L);
        _testService.SetState(shopAddress, shopState.Serialize());

        var order2 = OrderFactory.Create(agentAddress, avatarAddress, Guid.NewGuid(), 2 * currency, order.TradableId,
            1L,
            ItemSubType.Armor, 1);
        var shopAddress2 = ShardedShopStateV2.DeriveAddress(ItemSubType.Armor, order2.OrderId);
        var shopState2 = new ShardedShopStateV2(shopAddress2);
        var orderDigest2 = new OrderDigest(
            agentAddress,
            1L,
            Order.ExpirationInterval + 1L,
            order2.OrderId,
            order2.TradableId,
            order2.Price,
            0,
            0,
            item.Id,
            1
        );
        shopState2.Add(orderDigest2, 1L);
        _testService.SetState(Order.DeriveAddress(order2.OrderId), order2.Serialize());
        _testService.SetState(shopAddress2, shopState2.Serialize());

        await client.SyncOrder(ItemSubType.Armor, null!, _crystalEquipmentGrindingSheet,
            _crystalMonsterCollectionMultiplierSheet);
#pragma warning disable EF1001
        var nextContext = await contextFactory.CreateDbContextAsync(ct);
#pragma warning restore EF1001
        Assert.Equal(2, nextContext.Products.Count());
        var oldProduct = nextContext.Products.Single(p => p.ProductId == order.OrderId);
        Assert.Equal(1, oldProduct.Price);
        Assert.False(oldProduct.Exist);
        var newProduct = nextContext.Products.Single(p => p.ProductId == order2.OrderId);
        Assert.Equal(2, newProduct.Price);
        Assert.True(newProduct.Exist);

        // Buy order
    }

    private class TestClient : RpcClient
    {
        public TestClient(IOptions<RpcConfigOptions> options, ILogger<RpcClient> logger, Receiver receiver,
            IDbContextFactory<MarketContext> contextFactory, TestService service) : base(options, logger, receiver,
            contextFactory)
        {
            Service = service;
            Init = true;
        }
    }

    private class TestService : ServiceBase<IBlockChainService>, IBlockChainService
    {
        private IAccountStateDelta _states;

        public TestService()
        {
            _states = new ActionBase.AccountStateDelta(ImmutableDictionary<Address, IValue>.Empty,
                ImmutableDictionary<(Address, Currency), BigInteger>.Empty,
                ImmutableDictionary<Currency, BigInteger>.Empty);
        }

        public IBlockChainService WithOptions(CallOptions option)
        {
            throw new NotImplementedException();
        }

        public IBlockChainService WithHeaders(Metadata headers)
        {
            throw new NotImplementedException();
        }

        public IBlockChainService WithDeadline(DateTime deadline)
        {
            throw new NotImplementedException();
        }

        public IBlockChainService WithCancellationToken(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IBlockChainService WithHost(string host)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<bool> PutTransaction(byte[] txBytes)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<long> GetNextTxNonce(byte[] addressBytes)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<byte[]> GetState(byte[] addressBytes, byte[] blockHashBytes)
        {
            var address = new Address(addressBytes);
            var value = _states.GetState(address);
            if (value is null) throw new NullReferenceException();
            return new UnaryResult<byte[]>(new Codec().Encode(value));
        }

        public UnaryResult<byte[]> GetBalance(byte[] addressBytes, byte[] currencyBytes, byte[] blockHashBytes)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<byte[]> GetTip()
        {
            throw new NotImplementedException();
        }

        public UnaryResult<bool> SetAddressesToSubscribe(byte[] toByteArray, IEnumerable<byte[]> addressesBytes)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<bool> IsTransactionStaged(byte[] txidBytes)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<bool> ReportException(string code, string message)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<bool> AddClient(byte[] addressByte)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<bool> RemoveClient(byte[] addressByte)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<Dictionary<byte[], byte[]>> GetAvatarStates(IEnumerable<byte[]> addressBytesList,
            byte[] blockHashBytes)
        {
            throw new NotImplementedException();
        }

        public UnaryResult<Dictionary<byte[], byte[]>> GetStateBulk(IEnumerable<byte[]> addressBytesList,
            byte[] blockHashBytes)
        {
            var result = new Dictionary<byte[], byte[]>();
            foreach (var addressBytes in addressBytesList)
                try
                {
                    result[addressBytes] = GetState(addressBytes, blockHashBytes).ResponseAsync.Result;
                }
                catch (NullReferenceException)
                {
                }

            return new UnaryResult<Dictionary<byte[], byte[]>>(result);
        }

        public void SetOrder(Order order)
        {
            SetState(Order.DeriveAddress(order.OrderId), order.Serialize());
        }

        public void SetState(Address address, IValue value)
        {
            _states = _states.SetState(address, value);
        }
    }
}
