# Umbraco.DictionaryBuilder.VueI18N

[![Nuget](https://img.shields.io/nuget/v/Our.Umbraco.DictionaryBuilder.VueI18N)](https://www.nuget.org/packages/Our.Umbraco.DictionaryBuilder.VueI18N/)

Exposes a url (`/umbraco/dictionaries.json`) to get the dictionaries in a format that matches Vue I18n locale structure.
This will return all dictionaries in every language.

There are 2 querystring paramaters that can be added: 
- `locale` or `l`, to speficy the requested language translation.
- `prefix` or `p`, to specify a prefix, that all return dictionaries have in thier key.

Example of how to wire up the dictionaries into Vue I18n:
```
import Vue from 'vue'
import axios from 'axios'
import VueAxios from 'vue-axios'
import VueI18n from 'vue-i18n'

// Initialize Vue
Vue.use(VueAxios, axios)
Vue.use(VueI18n)

// Initialize Vue I18n
const lang = document.querySelector('html').getAttribute('lang')
export const i18n = new VueI18n({
    locale: lang, // set locale
    messages: {}
});

var app = new Vue({
    el: '#app',
    i18n,
    created() {
        axios.get("/umbraco/dictionaries.json?l=" + lang).then(function (response) {
            i18n.setLocaleMessage(lang, response.data)
        });
    }
});
```

To use dictionaries in Vue, just use the `$t`/`$tc` syntax in Vue I18n.

---

#### Naming convensions

To get a dictionary with a specifc key, prefix the key with `$`. 
So lets say, that we need the value of a dictionary item with the key `ItemKey`, use `$itemKey`

*Note: Remember to use Camel Casing, when specifing a key*

If we need the value of a dictionary item with the key `ItemKey`, that is a child node to the key `Parent`, 
use `parent.$itemKey`. 
So use `$` prefix, when we want the value, otherwise we will get the child object containing child dictionary items.

#### Format
See [Vue I18n Formatting](https://kazupon.github.io/vue-i18n/guide/formatting.html#formatting)

*Note: Remember to use Camel Casing, when specifing a key*

> Example dictionary value: "Apple"
```
$t('$apple')
// Returns: "Apple"
```

> Example dictionary value: "{0} apple"
```
$t('$apple', ['green'])
// Returns: "green apple"
```

#### Pluralization
See [Vue I18n Pluralization](https://kazupon.github.io/vue-i18n/guide/pluralization.html#pluralization)

*Note: Remember to use Camel Casing, when specifing a key*

> Example dictionary value "no apples | one apple | \{count\} apples"

```
$tc('$apple', 0)
// Returns: "no apples"
```
```
$tc('$apple', 1)
// Returns: "one apple"
```
```
$tc('$apple', 2)
// Returns: "2 apples"
```
```
$tc('$apple', 33)
// Returns: "33 apples"
```
___
> Example dictionary value: "no {0} apples | one {0} apple | \{count\} {0} apples"

```
$tc('$apple', 0, ['red'])
// Returns: "no red apples"
```
```
$tc('$apple', 1, ['red'])
// Returns: "one red apple"
```
```
$tc('$apple', 2, ['red'])
// Returns: "2 red apples"
```
```
$tc('$apple', 33, ['red'])
// Returns: "33 red apples"
```