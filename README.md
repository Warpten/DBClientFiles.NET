


# DBClientFiles.NET

<a href="https://scan.coverity.com/projects/warpten-dbclientfiles-net">
  <img alt="Coverity Scan Build Status"
       src="https://scan.coverity.com/projects/15970/badge.svg"/>
</a>

A blazing-fast DBC & DB2 reader for World of Warcraft's serialized database format.

# Usage

## Supported file types

|Signature|WDBC|WDB2|WDB3|WDB4|WDB5|WDB6|WDC1|WDC2|
|--|--|--|--|--|--|--|--|--|
|Read|:heavy_check_mark:|:heavy_check_mark:|:x:|:x:|❓|:x:|❓|:x:|
|Write|:x:|:x:|:x:|:x:|:x:|:x:|:x:|:x:|

:heavy_check_mark: Verified and tested.
:question: Implemented but not tested.
:x: Not supported.

WDB3 and WDB4 cannot be handled due to the files not being self-sufficient (~~and also because I'm lazy~~).
Write support is on the way.


DBClientFiles.NET exposes multiple collection types:
## Container types

### `StorageList<T>`
This container should be your default go-to when reading a file.
```cs
var storage = new StorageList<AchievementEntry>("Achievement.dbc");
```
This container implements the `IDisposable` and `IList<T>` interfaces.

### `StorageEnumerable<T>`
This container behaves pretty similarly to `StorageList<T>` with one simple difference: it provides lazily-loaded elements. It also implements the `IDisposable` and `IEnumerable<T>` interfaces.

```cs
var storageEnumerable = new StorageEnumerable<AchievementEntry>("Achievement.dbc");
```

### `StorageList`

:warning: This container is very much experimental and its API surface is subject to change.

This container is a loosely typed version of `StorageList<T>`. Instead of expecting the type of the record, it takes in a `DynamicStructure` and contains a collection of `Record`, indexed by their position within the file.

The `DynamicStructure` object itself is just a collection of implementations of the `IRecordReader` interface. 

An example speaks louder than a thousand words:

```cs
var storage = new StorageList(new DynamicStructure()
    .With(new RecordMember<int>("ID"))
    .With(new RecordMember<int>("MapID"))
    /* etc */), StorageOptions.Default, "Achievement.dbc");

var firstRecord = storage[0];
var mapID = firstRecord["MapID"].Integer;
```

## Attributes

### `CardinalityAttribute`

This attribute is used to indicate the size of an array *field* or *property*. Behavior depends on the file format:
- For **WDBC** and **WDB2**: this attribute is required.
- For **WDB5** and **WDB6**: the library is able to guess proper array sizes for the given members **except** the last one.
- For **WDC1** and **WDC2**: the library is able to guess proper array size for all given members.

If you don't know what you should be doing, decorate your arrays.

### `IgnoreAttribute`

This attribute is used to indicate that a field or property is to be ignored by the library. If the decorated member is either `readonly` or does not have a setter, this attribute is **redundant**.

### `IndexAttribute`

This attribute is used to decorate the member of a record that is it's key. Behavior varies depending on the file format:
- For **WDBC** and **WDB2** : If `IndexAttribute` is lacking, the first member is assumed to be the key.
- For **WDB5** and higher: This attribute is considered redundant as files already identify their index column.

### StorageOptions

- `MemberType`

  This is a simple way to tell the library you declared members as either **fields** or **properties**. Any other type obviously makes zero sense. This defaults to `MemberTypes.Property`.

- `LoadMask`

:warning: Not yet implemented.

  This property is a way to change the purpose of the library. Usually, you would want `LoadMask.Records`, but there are situations where you would open a file just to take a look at it's string table, or maybe just its structure. This property allows you to speed up execution by loading what you need. Defaults to `LoadMask.Records`.

- `InternStrings`

  By default, the CLR does not intern strings when they are not explicit in source. This means that any string that is in the file's string table and may be duplicated  will appear more than once in memory and use twice the space. 
  
  `DBClientFiles.NET` uses C#'s own internal string pool, which lives until the runtime terminates.
  
  :warning: Be careful with this. The performance impact of interning strings can be huge, since it leads to potentially unnecessary allocations, hash table lookups, and garbage collections. Use only with files where a majority of the strings are duplicates.

* `OverrideSignedChecks`

  Starting from **WDC1**, files contain additional information on their type, namely wether or not they are signed. If this property is set to `true` and one of your structure's declared member's signedness does not match the one specified by the file, an exception will be raised. This is otherwise ignored.

* `CopyToMemory`

  If set to true and the `Stream` provided to any of the types exposed by the library is not a `MemoryStream`, the library will instanciate a `MemoryStream` into which the input stream is copied. This can help when dealing with network streams (which are typically not seekable). This is ignored if the input stream does not support seek operations - in which case the `MemoryStream` mentionned above is created.

## Declaring structures - Quirks and tips

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
:warning: **WDB5** and **WDB6** records are aligned to the size of their largest member. This means that the library is able to calculate array sizes based solely on information provided by the file. However, this is the case for only a handful of files, and as such you should never rely on this behavior and at least specify the cardinality of the last member of the structure.

:information_source: On the other hand,  **WDC1** and **WDC2**  contain so much information about members that you almost don't require attributes to make your structure work:

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

Even then, `IndexAttribute` isn't actually required, since that information is contained within the file .

