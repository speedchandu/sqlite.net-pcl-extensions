﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SQLite.Net.Async;
using SQLiteNetExtensions.Extensions;
using System.Linq.Expressions;

#if USING_MVVMCROSS
using SQLiteConnection = Cirrious.MvvmCross.Community.Plugins.Sqlite.ISQLiteConnection;
#elif PCL
using SQLite.Net;
using SQLite.Net.Attributes;
#else
using SQLite;
#endif

namespace SQLiteNetExtensionsAsync.Extensions
{
    public static class ReadOperations
    {
        #region Public API

        /// <summary>
        /// Fetches all the entities of the specified type with the filter and fetches all the relationship
        /// properties of all the returned elements.
        /// </summary>
        /// <returns>List of all the elements of the type T that matches the filter with the children already loaded</returns>
        /// <param name="conn">SQLite Net connection object</param>
        /// <param name="filter">Filter that will be passed to the <c>Where</c> clause when fetching
        /// objects from the database. No relationship properties are allowed in this filter as they
        /// are loaded afterwards</param>
        /// <param name="recursive">If set to <c>true</c> all the relationships with
        /// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
        /// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
        public static async Task<List<T>> GetAllWithChildrenAsync<T>(this SQLiteAsyncConnection conn, Expression<Func<T, bool>> filter = null, bool recursive = false) where T : class
        {
            return await Task.Run(() =>
            {
                var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
                using (connectionWithLock.Lock())
                {
                    return connectionWithLock.GetAllWithChildren(filter, recursive);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Obtains the object from the database and fetches all the properties annotated with
        /// any subclass of <c>RelationshipAttribute</c>. If the object with the specified primary key doesn't
        /// exist in the database, an exception will be raised.
        /// </summary>
        /// <returns>The object with all the children loaded</returns>
        /// <param name="conn">SQLite Net connection object</param>
        /// <param name="pk">Primary key for the object to search in the database</param>
        /// <param name="recursive">If set to <c>true</c> all the relationships with
        /// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
        /// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
        public static async Task<T> GetWithChildrenAsync<T>(this SQLiteAsyncConnection conn, object pk, bool recursive = false) where T : class
        {
            return await Task.Run(() =>
            {
                var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
                using (connectionWithLock.Lock())
                {
                    return connectionWithLock.GetWithChildren<T>(pk, recursive);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// The behavior is the same that <c>GetWithChildren</c> but it returns null if the object doesn't
        /// exist in the database instead of throwing an exception
        /// Obtains the object from the database and fetch all the properties annotated with
        /// any subclass of <c>RelationshipAttribute</c>. If the object with the specified primary key doesn't
        /// exist in the database, it will return null
        /// </summary>
        /// <returns>The object with all the children loaded or null if it doesn't exist</returns>
        /// <param name="conn">SQLite Net connection object</param>
        /// <param name="pk">Primary key for the object to search in the database</param>
        /// <param name="recursive">If set to <c>true</c> all the relationships with
        /// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
        /// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
        public static async Task<T> FindWithChildrenAsync<T>(this SQLiteAsyncConnection conn, object pk, bool recursive = false) where T : class
        {
            return await Task.Run(() =>
            {
                var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
                using (connectionWithLock.Lock())
                {
                    return connectionWithLock.FindWithChildren<T>(pk, recursive);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches all the properties annotated with any subclass of <c>RelationshipAttribute</c> of the current
        /// object and keeps fetching recursively if the <c>recursive</c> flag has been set.
        /// </summary>
        /// <param name="conn">SQLite Net connection object</param>
        /// <param name="element">Element used to load all the relationship properties</param>
        /// <param name="recursive">If set to <c>true</c> all the relationships with
        /// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
        /// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
        public static async Task GetChildrenAsync<T>(this SQLiteAsyncConnection conn, T element, bool recursive = false)
        {
            await Task.Run(() =>
            {
                var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
                using (connectionWithLock.Lock())
                {
                    connectionWithLock.GetChildren(element, recursive);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a specific property of the current object and keeps fetching recursively if the
        /// <c>recursive</c> flag has been set.
        /// </summary>
        /// <param name="conn">SQLite Net connection object</param>
        /// <param name="element">Element used to load all the relationship properties</param>
        /// <param name="relationshipProperty">Name of the property to fetch from the database</param>
        /// <param name="recursive">If set to <c>true</c> all the relationships with
        /// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
        /// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
        public static async Task GetChildAsync<T>(this SQLiteAsyncConnection conn, T element, string relationshipProperty, bool recursive = false)
        {
            await Task.Run(() =>
            {
                var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
                using (connectionWithLock.Lock())
                {
                    connectionWithLock.GetChild(element, element.GetType().GetRuntimeProperty(relationshipProperty), recursive);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a specific property of the current object and keeps fetching recursively if the
        /// <c>recursive</c> flag has been set.
        /// </summary>
        /// <param name="conn">SQLite Net connection object</param>
        /// <param name="element">Element used to load all the relationship properties</param>
        /// <param name="propertyExpression">Expression that returns the property to be loaded from the database.
        /// This variant is useful to avoid spelling mistakes and make the code refactor-safe.</param>
        /// <param name="recursive">If set to <c>true</c> all the relationships with
        /// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
        /// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
        public static async Task GetChildAsync<T>(this SQLiteAsyncConnection conn, T element, Expression<Func<T, object>> propertyExpression, bool recursive = false)
        {
            await conn.GetChildAsync(element, ReflectionExtensions.GetProperty(propertyExpression), recursive).ConfigureAwait(false);
        }

        /// <summary>
        /// Fetches a specific property of the current object and keeps fetching recursively if the
        /// <c>recursive</c> flag has been set.
        /// </summary>
        /// <param name="conn">SQLite Net connection object</param>
        /// <param name="element">Element used to load all the relationship properties</param>
        /// <param name="relationshipProperty">Property to load from the database</param>
        /// <param name="recursive">If set to <c>true</c> all the relationships with
        /// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
        /// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
        public static async Task GetChildAsync<T>(this SQLiteAsyncConnection conn, T element, PropertyInfo relationshipProperty, bool recursive = false)
        {
            await Task.Run(() =>
            {
                var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
                using (connectionWithLock.Lock())
                {
                    connectionWithLock.GetChild(element, relationshipProperty, recursive);
                }
            }).ConfigureAwait(false);
        }

        #endregion

    }
}
