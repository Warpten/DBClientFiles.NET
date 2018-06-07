
# DBClientFiles.NET

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
|Read|:heavy_check_mark:|:heavy_check_mark:|:x:|:x:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|:heavy_check_mark:|
|Write|:x:|:x:|:x:|:x:|:x:|:x:|:x:|:x:|

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

### `StorageEnumerable<TKey, T>`
- `public StorageEnumerable(Stream fileStream)`
- `public StorageEnumerable(Stream fileStream, StorageOptions options)`

If `StorageOptions` is not provided, `StorageOptions.Default` is used instead.
:exclamation: This type is inherited by `StorageEnumerable<T>` where `TKey` is constrained to be of type `int`.

### `StorageList<TKey, T>`
- `public StorageList(Stream dataStream)`
- `public StorageList(Stream dataStream, StorageOptions options)`

If `StorageOptions` is not provided, `StorageOptions.Default` is used instead.
:exclamation: This type is inherited by `StorageList<T>` where `TKey` is constrained to be of type `int`.

### `StorageDictionary<...>`

Two different generic types with the name `StorageDictionary` exist:

 * `StorageDictionary<TKey, TFileKey, T>`
   * `public StorageDictionary(..., Func<TValue, TKey> keyGetter)`
   Creates an instance of the dictionary, using the provided lambda to store its result as a key.
   Example:
    ```cs
   public sealed class DummyStructure
   {
       [Index] public int ID { get; set; }
       public uint OtherIdentifier { get; set; }
       // ... data
   }
   ...
   var dictionary = new StorageDictionary<uint, int, DummyStructure>(..., dummyStructure => dummyStructure.OtherIdentifier);
   ```
   Note that `keyGetter` has no relation to the member decorated with `IndexAttribute`.
   A simpler, and possibly more efficient way to do this, is to use `StorageEnumerable` and LINQ:
   ```cs
   var enumerable = new StorageEnumerable<DummyStructure>(...);
   var dictionary = enumerable.ToDictionary(record => record.OtherIdentifier);
   ```
   Which may not be the same type but exposes the same interfaces (lest `IStorage`).
   
 * `StorageDictionary<TKey, T> : StorageDictionary<TKey, TKey, T>`
 This is a simpler type where the key defaults to the member decorated with `IndexAttribute`.

Obviously, for both types, if `StorageOptions` is not provided, `StorageOptions.Default` is used instead.
:exclamation: `StorageDictionary` is very much a work in progress and is continuously worked on.


### StorageOptions

    public sealed class StorageOptions
    {
    	public MemberTypes MemberType { get; set; } = MemberTypes.Property;
    	public LoadMask LoadMask { get; set; } = LoadMask.Records;
    
    	public bool InternStrings { get; set; } = true;
    	public bool KeepStringTable { get; set; } = false;
    
    	/// <summary>
    	/// If set to to <code>true</code>, the stream used as source will be copied to memory before being used.
    	/// This is set to true by default for anything but MemoryStream.
    	/// </summary>
    	public bool CopyToMemory { get; set; } = false;
    
    	public static StorageOptions Default { get; }
    }

`StorageOptions` allows you to define and control how a DBC/DB2 file is loaded from disk.
* `MemberType` is a simple way to tell the library that the matching members in your type are declared as either fields or properties. Anything else not supported.
* `LoadMask` will (in the future) be used when a user does not necessarily want to deserialize a file but instead have access to various elements of the file, such as the string table, the index table, or whatever they may like. This may prove useful for structure matching files across versions.
* `InternStrings`. By default, the CLR does not intern strings when they are not explicit in source. This means that any string that is in the file's string table and may be duplicated  will appear more than once in memory and use twice the space. 
`DBClientFiles.NET` uses C#'s own internal string pool, which lives until the runtime terminates.
:exclamation: Be careful with this. The performance impact of interning strings can be huge, since it leads to unnecessary allocations, hash table lookups, and garbage collections. Use only with files where a majority of the strings are duplicates.
* `KeepStringTable`: Unused.
* `CopyToMemory`: When loading from the provided `Stream` instance, if this property is set the true, the entirety of the stream's content will be loaded into a `MemoryStream` which will then be entirely managed by the collection. This can help for slow drives.
