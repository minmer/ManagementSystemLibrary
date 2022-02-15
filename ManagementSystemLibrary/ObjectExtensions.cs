// <copyright file="ObjectExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ManagementSystemLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using ManagementSystemLibrary.ManagementSystem;

    /// <summary>
    /// Represents extensions for several objects.
    /// </summary>
    public static class ObjectExtensions
    {
        private static readonly Dictionary<Type, string> Abbreviations = new ();
        private static readonly Dictionary<Type, byte[]> AbbreviationHashs = new ();

        /// <summary>
        /// Extens an array by another array.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="object">The array that should be extended.</param>
        /// <param name="extension">The array that is the extension.</param>
        /// <returns>The object if it is not null.</returns>
        public static ICollection<T> ExtendArray<T>(this T[] @object, T[] extension)
        {
            if (extension.Length > 0)
            {
                T[] newArray = new T[@object.Length + extension.Length];
                @object.CopyTo(newArray, 0);
                extension.CopyTo(newArray, @object.Length);
                return newArray;
            }

            return @object;
        }

        /// <summary>
        /// Returns a random <see cref="long"/>.
        /// </summary>
        /// <param name="random">The random used.</param>
        /// <returns>The random <see cref="long"/>.</returns>
        public static long NextLong(this Random random)
        {
            byte[] buffer = new byte[8];
            random.NextBytes(buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Trims the start of a <see cref="string"/> up to the first occurence of a <see cref="char"/>.
        /// </summary>
        /// <param name="input">The input <see cref="string"/>.</param>
        /// <param name="character">The <see cref="char"/> used for trimming.</param>
        /// <returns>The trimmed <see cref="string"/>.</returns>
        public static string TrimStartToFirstChar(this string input, char character)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            int index = input.IndexOf(character);
            if (index >= 0)
            {
                return input[(index + 1) ..];
            }

            return input;
        }

        /// <summary>
        /// Trims the start of a <see cref="string"/> up to the last occurence of a <see cref="char"/>.
        /// </summary>
        /// <param name="input">The input <see cref="string"/>.</param>
        /// <param name="character">The <see cref="char"/> used for trimming.</param>
        /// <returns>The trimmed <see cref="string"/>.</returns>
        public static string TrimStartToLastChar(this string input, char character)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            int index = input.LastIndexOf(character);
            if (index >= 0)
            {
                return input[(index + 1) ..];
            }

            return input;
        }

        /// <summary>
        /// Trims the end of a <see cref="string"/> up to the first occurence of a <see cref="char"/>.
        /// </summary>
        /// <param name="input">The input <see cref="string"/>.</param>
        /// <param name="character">The <see cref="char"/> used for trimming.</param>
        /// <returns>The trimmed <see cref="string"/>.</returns>
        public static string TrimEndToFirstChar(this string input, char character)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            int index = input.IndexOf(character);
            if (index >= 0)
            {
                return input[..index];
            }

            return input;
        }

        /// <summary>
        /// Trims the end of a <see cref="string"/> up to the last occurence of a <see cref="char"/>.
        /// </summary>
        /// <param name="input">The input <see cref="string"/>.</param>
        /// <param name="character">The <see cref="char"/> used for trimming.</param>
        /// <returns>The trimmed <see cref="string"/>.</returns>
        public static string TrimEndToLastChar(this string input, char character)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            int index = input.LastIndexOf(character);
            if (index >= 0)
            {
                return input[..index];
            }

            return input;
        }

        /// <summary>
        /// Imports into an <see cref="Aes"/> a key and iv.
        /// </summary>
        /// <param name="aes">The <see cref="Aes"/> where the key and iv should be imported.</param>
        /// <param name="key">The key to import.</param>
        /// <param name="iv">The iv to import.</param>
        /// <returns>The input <see cref="Aes"/>.</returns>
        public static Aes ImportKey(this Aes aes, byte[] key, byte[] iv)
        {
            aes.Key = key;
            aes.IV = iv;
            return aes;
        }

        /// <summary>
        /// Gets the abbreviation of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>The abbreviation of the <see cref="Type"/>.</returns>
        public static string GetDatabaseAbbreviation(this Type type)
        {
            if (!Abbreviations.ContainsKey(type))
            {
                Abbreviations.Add(type, type.Name.ToLower());
                AbbreviationHashs.Add(type, Encoding.Unicode.GetBytes(Abbreviations[type]));
            }

            return Abbreviations[type];
        }

        /// <summary>
        /// Gets the abbreviation hash of the <see cref="MSDatabaseObject"/>.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>The abbreviation of the <see cref="Type"/>.</returns>
        public static byte[] GetDatabaseAbbreviationHash(this Type type)
        {
            if (!Abbreviations.ContainsKey(type))
            {
                Abbreviations.Add(type, type.Name.ToLower());
                AbbreviationHashs.Add(type, Encoding.Unicode.GetBytes(Abbreviations[type]));
            }

            return AbbreviationHashs[type];
        }

        /// <summary>
        /// Loads the parents of an <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        /// <param name="linkObjects">The linkobject.</param>
        /// <typeparam name="T">The type of the linkobjects.</typeparam>
        /// <typeparam name="TParent">The type of the parent.</typeparam>
        /// <typeparam name="TChild">The type of the child.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<IEnumerable<TParent>> LoadParentsAsync<T, TParent, TChild>(this IEnumerable<T> linkObjects)
            where T : MSLinkObject<TChild, TParent>
            where TParent : MSAccessObject
            where TChild : MSAccessObject
        {
            TParent[] parents = new TParent[linkObjects.Count()];
            for (int index = 0; index < parents.Length; index++)
            {
                parents[index] = await linkObjects.ElementAt(index).GetParentAsync().ConfigureAwait(false) ?? throw new NullReferenceException();
            }

            return parents;
        }

        /// <summary>
        /// Loads the parents of an <see cref="MSLinkObject{T,T}"/>.
        /// </summary>
        /// <param name="linkObjects">The linkobject.</param>
        /// <typeparam name="T">The type of the linkobjects.</typeparam>
        /// <typeparam name="TParent">The type of the parent.</typeparam>
        /// <typeparam name="TChild">The type of the child.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<IEnumerable<TChild>> LoadChildrenAsync<T, TParent, TChild>(this IEnumerable<T> linkObjects)
            where T : MSLinkObject<TChild, TParent>
            where TParent : MSAccessObject
            where TChild : MSAccessObject
        {
            TChild[] children = new TChild[linkObjects.Count()];
            for (int index = 0; index < children.Length; index++)
            {
                children[index] = await linkObjects.ElementAt(index).GetChildAsync().ConfigureAwait(false) ?? throw new NullReferenceException();
            }

            return children;
        }
    }
}
