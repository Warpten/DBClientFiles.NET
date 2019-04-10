

# DBClientFiles.NET

<a href="https://scan.coverity.com/projects/warpten-dbclientfiles-net">
  <img alt="Coverity Scan Build Status"
       src="https://scan.coverity.com/projects/15970/badge.svg"/>
</a>

A blazing-fast DBC & DB2 reader for World of Warcraft's serialized database format.

## Usage

DBClientFiles.NET exposes three collection types:

 - `StorageList`
 - `StorageDictionary` 
 - `StorageEnumerable`

Each exposed type requires `T : class, new()` and `TKey : struct`, and is constructed with `System.IO.Stream` and optionally `StorageOptions` instances. If `StorageOptions` is not provided, the library automatically uses `StorageOptions.Default`.

As well as being collections, these types implement `IStorage`, which exposes metadata information about the file that was deserialized, such as **signature**, **table hash** and **layout hash**. 

## Supported file types

|Signature|WDBC|WDB2|WDB3|WDB4|WDB5|WDB6|WDC1|WDC2|
|--|--|--|--|--|--|--|--|--|
|Read|:heavy_check_mark:|:heavy_check_mark:|:x:|:x:|:x:|:x:|:x:|:x:|
|Write|:x:|:x:|:x:|:x:|:x:|:x:|:x:|:x:|

:heavy_check_mark: Verified and tested.
:question: Implemented but not tested.
:x: Not supported.

WDB3 and WDB4 cannot be handled due to the files not being self-sufficient (~~and also because I'm lazy~~).
Write support is on the way.

## Attributes

### `CardinalityAttribute`

This attribute is used to indicate the size of an array *field* or *property*. Behavior depends on the file format:
- For **WDBC** and **WDB2**: this attribute is required.
- For **WDB5** and **WDB6**: the library is able to guess proper array sizes for the given members **except** the last one.
- For **WDC1** and **WDC2**: the library is able to guess proper array size for all given members.

If you don't know what you should be doing, decorate your arrays.

### `IgnoreAttribute`

This attribute is used to indicate that a field or property is to be ignored by the library. It is **redundant** as the library already does sanity checks and **ignores** `readonly` fields and properties without setter.

### `IndexAttribute`

This attribute is used to decorate the member of a record that is it's key. Behavior varies depending on the file format:
- For **WDBC** and **WDB2** : If `IndexAttribute` is lacking, the first member is assumed to be the key.
- For **WDB5** and higher: This attribute is considered redundant as files already identify their index column.

## API

### `StorageEnumerable<T>`
- `public StorageEnumerable(Stream fileStream)`
- `public StorageEnumerable(Stream fileStream, StorageOptions options)`

If `StorageOptions` is not provided, `StorageOptions.Default` is used instead.

### `StorageList<T>`
- `public StorageList(Stream dataStream)`
Forwards to the following constructor, using `StorageOptions.Default`.
- `public StorageList(Stream dataStream, StorageOptions options)`

### StorageOptions

- `MemberType`

  This is a simple way to tell the library you declared members as either **fields** or **properties**. Any other type obviously makes zero sense. This defaults to `MemberTypes.Property`.

- `LoadMask`

  This property is a way to change the purpose of the library. Usually, you would want `LoadMask.Records`, but there are situations where you would open a file just to take a look at it's string table, or maybe just its structure. This property allows you to speed up execution by loading what you need. Defaults to `LoadMask.Records`.

- `InternStrings`

  By default, the CLR does not intern strings when they are not explicit in source. This means that any string that is in the file's string table and may be duplicated  will appear more than once in memory and use twice the space. 
  
  `DBClientFiles.NET` uses C#'s own internal string pool, which lives until the runtime terminates.
  
  :exclamation: Be careful with this. The performance impact of interning strings can be huge, since it leads to unnecessary allocations, hash table lookups, and garbage collections. Use only with files where a majority of the strings are duplicates.

* `OverrideSignedChecks`

  Starting from *WDC1*, files contain additional information on their type, namely wether or not they are signed. Use this property to avoid enforced checks on the types you declared in your file. Defaults to `false`.

* `CopyToMemory`

  If set to true and the `Stream` provided to any of the types exposed by the library is not a `MemoryStream`, the library will instanciate a `MemoryStream` into which the input stream is copied. This can help when dealing with network streams (which are typically not seekable). This is ignored if the input stream does not support seek operations - in which case the `MemoryStream` mentionned above is created.
 
## Usage

### Declaring structures - Quirks and tips

:information_source: Older formats such as `WDBC` and `WDB2` do not contain much informations about fields, except their amount (including array sizes). This means that you should be very verbose about your structures:

```cs
public sealed class AchievementEntry
{
    [Index]
    public int ID { get; set; }
    ...
    [Cardinality(SizeConst = 16)]
    public string[] Title { get; set; }
    ...
    [Cardinality(SizeConst = 16)]
    public string[] Description { get; set; }
    ...
    [Cardinality(SizeConst = 16)]
    public string[] Rewards { get; set; }
    ...
}
```

This is not the case with `WDB5` and `WDB6`, to some extend. The library is able to guess the cardinality of arrays if they are not the last member of the structure, because that information is contained in the file.

:information_source: On the other hand, `WDC1` and `WDC2` contain so much information about members that you almost don't require attributes to make your structure work:

```cs
public sealed class AchievementEntry // WDC1
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Reward { get; set; }
    public int Flags { get; set; }
    public short InstanceID { get; set; }
    public short Supercedes { get; set; }
    public short Category { get; set; }
    public short UiOrder { get; set; }
    public short SharesCriteria { get; set; }
    public byte Faction { get; set; }
    public byte Points { get; set; }
    public byte MinimumCriteria { get; set; }
    [Index]
    public int ID { get; set; }
    public int IconFileID { get; set; }
    public uint CriteriaTree { get; set; }
}
```

Even then, `IndexAttribute` isn't actually required.

:exclamation: :exclamation: The library does not support substructures arrays. They will compile, but you will get an `InvalidStructureException` at runtime:

```cs
public struct C3Vector // Can also be a class
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

// This is fine
public sealed class SomeFile
{
    public int ID { get; set; }
    public C3Vector Position { get; set; }
}

// Runtime InvalidStructureException
public sealed class SomeFile
{
    public int ID { get; set; }
    public C3Vector[] Positions { get; set; }
}
```

:information_source: It is also possible to nest substructures:
```cs
public struct C2Vector
{
    public float X { get; set; }
    public float Y { get; set; }
}

public struct C3Vector
{
    public C2Vector XY { get; set;} // Ignored if private, encapsulation is respected.
    public float Z { get; }
    public float X => XY.X; // These are skipped because there are no setter.
    [Ignore] // Explicitely skipping - redundant here.
    public float Y => XY.Y; 
}

public sealed class Structure
{
    public int ID { get; set; }
    public C3Vector Position { get; set; }
}
```
