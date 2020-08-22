# Umbraco.DictionaryBuilder
An Umbraco-CMS strongly typed dictionary item builder

[![Nuget](https://img.shields.io/nuget/v/Our.Umbraco.DictionaryBuilder)](https://www.nuget.org/packages/Our.Umbraco.DictionaryBuilder/)

## Usage

To write a dictionary item value in a Razor file, just write

> Dictionary value "Apple"
```
@Dictionaries.Apple
// Returns: "Apple"
```

Dictionary values are return in CurrentUICulture, unless another culture is specified in the [Format extension method](#format-extensions). 

### Format extensions

In namespace `Umbraco.DictionaryBuilder.Extensions`

#### string Format(params object[] args)

> Dictionary value "{0} apple"
```
@Dictionaries.Apple.Format("green")
// Returns: "green apple"
```
Equivalent to `@string.Format(Dictionaries.Apple)`


#### string Format(int count, params object[] args)
A pluralization method

> Dictionary value "no apples|one apple|\{count\} apples"

```
@Dictionaries.Apple.Format(0)
// Returns: "no apples"
```
```
@Dictionaries.Apple.Format(1)
// Returns: "one apple"
```
```
@Dictionaries.Apple.Format(2)
// Returns: "2 apples"
```
```
@Dictionaries.Apple.Format(33)
// Returns: "33 apples"
```
___
> Dictionary value "no {0} apples|one {0} apple|\{count\} {0} apples"

```
@Dictionaries.Apple.Format(0, "red")
// Returns: "no red apples"
```
```
@Dictionaries.Apple.Format(1, "red")
// Returns: "one red apple"
```
```
@Dictionaries.Apple.Format(2, "red")
// Returns: "2 red apples"
```
```
@Dictionaries.Apple.Format(33, "red")
// Returns: "33 red apples"
```

#### string Format(CultureInfo culture, params object[] args)
Like [string Format(params object[] args)](#string-formatparams-object-args), but in the requested culture.

#### string Format(CultureInfo culture, int count, params object[] args)
Like [string Format(int count, params object[] args)](#string-formatint-count-params-object-args), but in the requested culture.

## Configuration

### AppSettings

#### Umbraco.DictionaryBuilder.ModelsMode
Default value: `LiveAppData`

#### Umbraco.DictionaryBuilder.DictionaryNamespace
The namespace in witch the dictionary models will be generated.

> Default value: `Umbraco.Web`

#### Umbraco.DictionaryBuilder.DictionaryItemsPartialClassName
The name of the partial class that contains all the generated dictionary models.

> Default value: `Dictionaries`

#### Umbraco.DictionaryBuilder.DictionaryDirectory
The relative path to the folder where to generate the dictionary models

>  Default value: `~/App_Data/Dictionaries`

#### Umbraco.DictionaryBuilder.AcceptUnsafeModelsDirectory
An indicator of whether is accepted to generate dictionary models in a folder outside of the current project.

> Valid values: `True`/`False`
>
> Default value: `False`

#### Umbraco.DictionaryBuilder.UseNestedStructure
Todo: Explain this

> Valid values: `True`/`False`
>
> Default value: `False`

#### Umbraco.DictionaryBuilder.GenerateFilePerDictionaryItem
An indicator of whether the dictionary models should be generated in a single file or a file per dictionary item.

> Valid values: `True`/`False`
>
> Default value: `False`

#### Umbraco.DictionaryBuilder.Enable
An indicator of whether the DictionaryBuilder is enabled.

> Valid values: `True`/`False`
> 
> Default value: `False`

# Umbraco.DictionaryBuilder.VueI18N

[![Nuget](https://img.shields.io/nuget/v/Our.Umbraco.DictionaryBuilder.VueI18N)](https://www.nuget.org/packages/Our.Umbraco.DictionaryBuilder.VueI18N/)

Todo: Explain this