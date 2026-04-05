using System;
using System.Linq;
using Wanderer.Shared;
using Wanderer.Shared.Models.Profile;

namespace Wanderer.Extensions;

public static class PersonExtensions
{
    public static bool IsMatch(this Person person, string query)
    {
        const StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

        return person.Name.Contains(query, ignoreCase)
               || person.Id.Contains(query, ignoreCase)
               || PinyinHelper.GetFullPinyinList(person.Name)
                   .Any(pinyin => pinyin.StartsWith(query, ignoreCase))
               || PinyinHelper.GetFirstPinyinList(person.Name)
                   .Any(pinyin => pinyin.StartsWith(query, ignoreCase));
    }
}