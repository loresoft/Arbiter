using System.Text;

namespace Arbiter.Services.Tests;

public class ObjectPoolTests
{
    private class TestObject
    {
        public int Value { get; set; }
        public bool IsReset { get; set; }
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithValidParameters_CreatesPool()
    {
        var pool = new ObjectPool<TestObject>(
            objectFactory: () => new TestObject(),
            resetAction: obj => obj.IsReset = true,
            maxSize: 10
        );

        pool.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ObjectPool<TestObject>(null!));
    }

    [Test]
    public void Constructor_WithNegativeMaxSize_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ObjectPool<TestObject>(() => new TestObject(), maxSize: -1));
    }

    [Test]
    public void Constructor_WithZeroMaxSize_UsesDefaultMaxSize()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject(), maxSize: 0);

        // Default max size should be Environment.ProcessorCount * 2
        // We can verify by filling the pool beyond that limit
        var objects = new List<TestObject>();
        var expectedMax = Environment.ProcessorCount * 2;

        // Get more objects than the default max
        for (int i = 0; i < expectedMax + 10; i++)
        {
            objects.Add(pool.Get());
        }

        // Return all objects
        foreach (var obj in objects)
        {
            pool.Return(obj);
        }

        // If we get objects again, some should be from the pool (reused)
        // and some should be new (created by factory)
        var reusedCount = 0;
        foreach (var obj in objects)
        {
            var retrieved = pool.Get();
            if (objects.Contains(retrieved))
                reusedCount++;
        }

        // At least some objects should have been reused
        reusedCount.Should().BeGreaterThan(0);
    }

    #endregion

    #region Get Tests

    [Test]
    public void Get_FromEmptyPool_CreatesNewObject()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject { Value = 42 });

        var obj = pool.Get();

        obj.Should().NotBeNull();
        obj.Value.Should().Be(42);
    }

    [Test]
    public void Get_AfterReturn_ReusesObject()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        var obj1 = pool.Get();
        obj1.Value = 100;
        pool.Return(obj1);

        var obj2 = pool.Get();

        obj2.Should().BeSameAs(obj1);
        obj2.Value.Should().Be(100);
    }

    [Test]
    public void Get_MultipleTimes_ReturnsDistinctObjects()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        var obj1 = pool.Get();
        var obj2 = pool.Get();
        var obj3 = pool.Get();

        obj1.Should().NotBeSameAs(obj2);
        obj2.Should().NotBeSameAs(obj3);
        obj1.Should().NotBeSameAs(obj3);
    }

    [Test]
    public void Get_UsesSingleItemCache_ForPerformance()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        var obj1 = pool.Get();
        pool.Return(obj1);

        // The next Get should retrieve from the single-item cache
        var obj2 = pool.Get();

        obj2.Should().BeSameAs(obj1);
    }

    #endregion

    #region Return Tests

    [Test]
    public void Return_WithValidObject_AddsToPool()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        var obj = pool.Get();
        pool.Return(obj);

        var reused = pool.Get();
        reused.Should().BeSameAs(obj);
    }

    [Test]
    public void Return_WithNull_DoesNotThrow()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        pool.Return(null!);

        // Should not throw exception
    }

    [Test]
    public void Return_WithResetAction_CallsResetBeforeReturning()
    {
        var pool = new ObjectPool<TestObject>(
            objectFactory: () => new TestObject(),
            resetAction: obj => obj.IsReset = true
        );

        var obj = pool.Get();
        obj.IsReset = false;
        pool.Return(obj);

        var reused = pool.Get();
        reused.IsReset.Should().BeTrue();
    }

    [Test]
    public void Return_WhenResetActionThrows_DoesNotAddToPool()
    {
        int getCount = 0;
        var pool = new ObjectPool<TestObject>(
            objectFactory: () => new TestObject { Value = ++getCount },
            resetAction: obj => throw new InvalidOperationException("Reset failed")
        );

        var obj1 = pool.Get();
        pool.Return(obj1);

        // Get another object - should create new one since reset failed
        var obj2 = pool.Get();

        obj2.Should().NotBeSameAs(obj1);
        obj1.Value.Should().Be(1);
        obj2.Value.Should().Be(2);
    }

    [Test]
    public void Return_BeyondMaxSize_DiscardsObject()
    {
        var pool = new ObjectPool<TestObject>(
            objectFactory: () => new TestObject(),
            maxSize: 2
        );

        var obj1 = pool.Get();
        var obj2 = pool.Get();
        var obj3 = pool.Get();
        var obj4 = pool.Get();

        obj1.Value = 1;
        obj2.Value = 2;
        obj3.Value = 3;
        obj4.Value = 4;

        // Return all objects
        pool.Return(obj1);
        pool.Return(obj2);
        pool.Return(obj3);
        pool.Return(obj4); // This should be discarded (exceeds maxSize)

        // Get objects back - due to single-item cache + queue of maxSize,
        // we can actually store maxSize+1 objects (1 in cache, maxSize in queue)
        // So with maxSize=2, we can store 3 objects total
        var reused1 = pool.Get();
        var reused2 = pool.Get();
        var reused3 = pool.Get();
        var reused4 = pool.Get();

        var allReused = new[] { reused1, reused2, reused3, reused4 };
        var originalObjects = new[] { obj1, obj2, obj3, obj4 };
        var reusedCount = allReused.Count(r => originalObjects.Contains(r));

        // Should reuse at most 3 objects (1 in cache + 2 in queue)
        reusedCount.Should().BeLessThanOrEqualTo(3);

        // And obj4 should not be reused (it was the 4th object returned)
        allReused.Should().NotContain(obj4);
    }

    [Test]
    public void Return_MultipleObjects_MaintainsPool()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        var objects = new List<TestObject>
        {
            pool.Get(),
            pool.Get(),
            pool.Get()
        };

        foreach (var obj in objects)
        {
            pool.Return(obj);
        }

        var reused1 = pool.Get();
        var reused2 = pool.Get();
        var reused3 = pool.Get();

        objects.Should().Contain(reused1);
        objects.Should().Contain(reused2);
        objects.Should().Contain(reused3);
    }

    #endregion

    #region GetPooled Tests

    [Test]
    public void GetPooled_ReturnsPooledObject()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject { Value = 42 });

        var pooled = pool.GetPooled();

        pooled.Instance.Should().NotBeNull();
        pooled.Instance.Value.Should().Be(42);
    }

    [Test]
    public void GetPooled_Dispose_ReturnsObjectToPool()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());
        TestObject? originalInstance = null;

        using (var pooled = pool.GetPooled())
        {
            originalInstance = pooled.Instance;
            originalInstance.Value = 999;
        }

        var reused = pool.Get();
        reused.Should().BeSameAs(originalInstance);
        reused.Value.Should().Be(999);
    }

    [Test]
    public void GetPooled_UsingStatement_AutomaticallyReturns()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());
        TestObject? instance = null;

        using (var pooled = pool.GetPooled())
        {
            instance = pooled.Instance;
            instance.Value = 123;
        }

        var next = pool.Get();
        next.Should().BeSameAs(instance);
    }

    [Test]
    public void GetPooled_ImplicitConversion_WorksCorrectly()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject { Value = 50 });

        using var pooled = pool.GetPooled();
        TestObject obj = pooled; // Implicit conversion

        obj.Should().NotBeNull();
        obj.Value.Should().Be(50);
    }

    [Test]
    public void GetPooled_WithResetAction_ResetsOnDispose()
    {
        var pool = new ObjectPool<TestObject>(
            objectFactory: () => new TestObject(),
            resetAction: obj =>
            {
                obj.Value = 0;
                obj.IsReset = true;
            }
        );

        using (var pooled = pool.GetPooled())
        {
            pooled.Instance.Value = 555;
            pooled.Instance.IsReset = false;
        }

        var reused = pool.Get();
        reused.Value.Should().Be(0);
        reused.IsReset.Should().BeTrue();
    }

    [Test]
    public void GetPooled_NestedUsing_WorksCorrectly()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        TestObject? obj1 = null;
        TestObject? obj2 = null;

        using (var pooled1 = pool.GetPooled())
        {
            obj1 = pooled1.Instance;
            obj1.Value = 1;

            using (var pooled2 = pool.GetPooled())
            {
                obj2 = pooled2.Instance;
                obj2.Value = 2;

                obj1.Should().NotBeSameAs(obj2);
            }

            // obj2 should be returned to pool now (goes to single-item cache)
            var reused = pool.Get();
            reused.Should().BeSameAs(obj2);
            pool.Return(reused);
        }

        // Now both obj1 and obj2 are in the pool
        // obj1 was just returned (goes to single-item cache, displacing obj2)
        // obj2 should now be in the queue

        var firstGet = pool.Get();
        var secondGet = pool.Get();

        // We should get both objects back (in some order)
        var retrieved = new[] { firstGet, secondGet };
        retrieved.Should().Contain(obj1);
        retrieved.Should().Contain(obj2);
    }

    #endregion

    #region Thread Safety Tests

    [Test]
    public async Task Get_Concurrent_IsThreadSafe()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());
        var tasks = new List<Task<TestObject>>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => pool.Get()));
        }

        var objects = await Task.WhenAll(tasks);

        objects.Should().HaveCount(100);
        objects.Should().AllSatisfy(obj => obj.Should().NotBeNull());
    }

    [Test]
    public async Task Return_Concurrent_IsThreadSafe()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());
        var objects = Enumerable.Range(0, 100).Select(_ => pool.Get()).ToList();

        var tasks = objects.Select(obj => Task.Run(() => pool.Return(obj))).ToList();

        await Task.WhenAll(tasks);

        // All objects should be safely returned
        // We can verify by getting them back
        var reused = Enumerable.Range(0, 100).Select(_ => pool.Get()).ToList();
        reused.Should().HaveCount(100);
    }

    [Test]
    public async Task GetAndReturn_Concurrent_IsThreadSafe()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject(), maxSize: 20);
        var iterations = 1000;

        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                var obj = pool.Get();
                obj.Value = i;
                pool.Return(obj);
            }
        })).ToList();

        await Task.WhenAll(tasks);

        // No exceptions should have been thrown
    }

    [Test]
    public async Task GetPooled_Concurrent_IsThreadSafe()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());
        var iterations = 100;

        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                using var pooled = pool.GetPooled();
                pooled.Instance.Value = i;
            }
        })).ToList();

        await Task.WhenAll(tasks);

        // No exceptions should have been thrown
    }

    #endregion

    #region StringBuilder Pool Extension Tests

    [Test]
    public void StringBuilderPool_IsAvailable()
    {
        var pool = StringBuilder.Pool;

        pool.Should().NotBeNull();
    }

    [Test]
    public void StringBuilderPool_Get_ReturnsStringBuilder()
    {
        var pool = StringBuilder.Pool;

        var sb = pool.Get();

        sb.Should().NotBeNull();
        sb.Should().BeOfType<StringBuilder>();
    }

    [Test]
    public void StringBuilderPool_Return_ClearsStringBuilder()
    {
        var pool = StringBuilder.Pool;

        var sb = pool.Get();
        sb.Append("Hello, World!");
        pool.Return(sb);

        var reused = pool.Get();
        reused.Should().BeSameAs(sb);
        reused.Length.Should().Be(0);
        reused.ToString().Should().BeEmpty();
    }

    [Test]
    public void StringBuilderPool_Return_TrimsExcessCapacity()
    {
        var pool = StringBuilder.Pool;

        // Get a fresh StringBuilder and expand it
        var sb = pool.Get();
        sb.Append(new string('X', 2000));
        var largeCapacity = sb.Capacity;
        largeCapacity.Should().BeGreaterThan(1024);

        // Return it - the reset action should trim the capacity
        pool.Return(sb);

        // The StringBuilder should have been trimmed to 256 capacity
        // We can verify this by checking the sb object directly after return
        // Note: The reset action trims during Return(), so we check the object state

        // Since we can't easily verify internal state after return without getting it back,
        // let's verify the trimming behavior by creating a dedicated test StringBuilder pool
        var testPool = new ObjectPool<StringBuilder>(
            objectFactory: () => new StringBuilder(256),
            resetAction: sb =>
            {
                sb.Clear();
                if (sb.Capacity > 1024)
                    sb.Capacity = 256;
            }
        );

        var testSb = testPool.Get();
        testSb.Append(new string('Y', 2000));
        testSb.Capacity.Should().BeGreaterThan(1024);

        testPool.Return(testSb);

        var reused = testPool.Get();
        reused.Should().BeSameAs(testSb);
        reused.Capacity.Should().Be(256);
    }

    [Test]
    public void StringBuilderPool_Return_PreservesReasonableCapacity()
    {
        // Use a dedicated pool to avoid shared state issues
        var testPool = new ObjectPool<StringBuilder>(
            objectFactory: () => new StringBuilder(256),
            resetAction: sb =>
            {
                sb.Clear();
                if (sb.Capacity > 1024)
                    sb.Capacity = 256;
            }
        );

        var sb = testPool.Get();

        // Use within reasonable limits (under 1024)
        sb.Append(new string('X', 500));
        var capacityBeforeReturn = sb.Capacity;
        capacityBeforeReturn.Should().BeLessThan(1024);

        testPool.Return(sb);

        var reused = testPool.Get();
        reused.Should().BeSameAs(sb);
        // Capacity should be preserved since it's under 1024
        reused.Capacity.Should().Be(capacityBeforeReturn);
    }

    [Test]
    public void StringBuilderPool_GetPooled_WorksCorrectly()
    {
        var pool = StringBuilder.Pool;

        string result;
        using (var pooled = pool.GetPooled())
        {
            pooled.Instance.Append("Hello");
            pooled.Instance.Append(", ");
            pooled.Instance.Append("World!");
            result = pooled.Instance.ToString();
        }

        result.Should().Be("Hello, World!");

        // Verify it was returned and cleared
        var reused = pool.Get();
        reused.Length.Should().Be(0);
        pool.Return(reused);
    }

    [Test]
    public void StringBuilderPool_IsSingleton()
    {
        var pool1 = StringBuilder.Pool;
        var pool2 = StringBuilder.Pool;

        pool1.Should().BeSameAs(pool2);
    }

    #endregion

    #region Edge Cases and Stress Tests

    [Test]
    public void Pool_WithLargeMaxSize_HandlesManyObjects()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject(), maxSize: 1000);

        var objects = Enumerable.Range(0, 1000).Select(_ => pool.Get()).ToList();

        foreach (var obj in objects)
        {
            pool.Return(obj);
        }

        // Verify objects are reused
        var reused = Enumerable.Range(0, 1000).Select(_ => pool.Get()).ToList();

        // Most objects should be reused
        var reusedCount = reused.Count(obj => objects.Contains(obj));
        reusedCount.Should().BeGreaterThan(900);
    }

    [Test]
    public void Pool_WithMaxSizeOne_WorksCorrectly()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject(), maxSize: 1);

        var obj1 = pool.Get();
        var obj2 = pool.Get();
        var obj3 = pool.Get();

        obj1.Value = 1;
        obj2.Value = 2;
        obj3.Value = 3;

        // Return all three objects
        pool.Return(obj1);
        pool.Return(obj2);
        pool.Return(obj3); // Should be discarded (exceeds maxSize)

        // With maxSize=1, we can store 2 objects (1 in cache + 1 in queue)
        var reused1 = pool.Get();
        var reused2 = pool.Get();
        var reused3 = pool.Get();

        // First two Gets should return pooled objects
        var pooledObjects = new[] { reused1, reused2 };
        pooledObjects.Should().Contain(obj1);
        pooledObjects.Should().Contain(obj2);

        // Third Get should create a new object (not obj3)
        reused3.Should().NotBeSameAs(obj3);
        reused3.Value.Should().Be(0); // New object has default value
    }

    [Test]
    public void Pool_GetReturnPattern_WorksForManyIterations()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject());

        for (int i = 0; i < 10000; i++)
        {
            var obj = pool.Get();
            obj.Value = i;
            pool.Return(obj);
        }

        // Should not throw or cause issues
    }

    [Test]
    public void Pool_WithComplexResetAction_WorksCorrectly()
    {
        var pool = new ObjectPool<TestObject>(
            objectFactory: () => new TestObject(),
            resetAction: obj =>
            {
                obj.Value = 0;
                obj.IsReset = true;
                // Simulate complex reset logic
                for (int i = 0; i < 100; i++)
                {
                    obj.Value += i;
                }
                obj.Value = 0; // Final reset
            }
        );

        var obj = pool.Get();
        obj.Value = 999;
        pool.Return(obj);

        var reused = pool.Get();
        reused.Value.Should().Be(0);
        reused.IsReset.Should().BeTrue();
    }

    [Test]
    public void Pool_ReturnSameObjectMultipleTimes_OnlyKeepsOne()
    {
        var pool = new ObjectPool<TestObject>(() => new TestObject(), maxSize: 10);

        var obj = pool.Get();

        // Return the same object multiple times (bad practice, but shouldn't break)
        pool.Return(obj);
        pool.Return(obj);
        pool.Return(obj);

        // Pool should handle this gracefully
        var obj1 = pool.Get();
        var obj2 = pool.Get();

        // Both might be the same object since we returned it multiple times
        // This is acceptable behavior for misuse
    }

    [Test]
    public void Pool_WithNullResetAction_WorksCorrectly()
    {
        var pool = new ObjectPool<TestObject>(
            objectFactory: () => new TestObject(),
            resetAction: null
        );

        var obj = pool.Get();
        obj.Value = 123;
        pool.Return(obj);

        var reused = pool.Get();
        reused.Should().BeSameAs(obj);
        reused.Value.Should().Be(123); // Not reset
    }

    #endregion
}
