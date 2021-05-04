using Synapse.Domain.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synapse.Domain
{

    /// <summary>
    /// Represents a domain-related <see cref="Exception"/>
    /// </summary>
    public class DomainException
        : Exception
    {

        /// <summary>
        /// Initializes a new <see cref="DomainException"/>
        /// </summary>
        public DomainException()
        {

        }

        /// <summary>
        /// Initializes a new <see cref="DomainException"/>
        /// </summary>
        /// <param name="message">The <see cref="Exception"/> message</param>
        public DomainException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new <see cref="DomainException"/>
        /// </summary>
        /// <param name="message">The <see cref="Exception"/> message</param>
        /// <param name="innerException">The inner <see cref="Exception"/></param>
        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> thrown when an argument is a date in the future
        /// </summary>
        /// <param name="argumentName">The name of the argument</param>
        /// <returns>A new <see cref="DomainArgumentException"/> thrown when an argument is a date in the future</returns>
        public static DomainArgumentException ArgumentCannotBeADateInTheFuture(string argumentName)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentcannotbeadateinthefuture, argumentName), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for a null argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for a null argument</returns>
        public static DomainArgumentException ArgumentNull(string argumentName)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentnull, argumentName), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for a null or whitespace argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for a null or whitespace argument</returns>
        public static DomainArgumentException ArgumentNullOrWhitespace(string argumentName)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentnullorwhitespace, argumentName), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid address argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid address</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid address argument</returns>
        public static DomainArgumentException ArgumentIsInvalidAddress(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidaddress, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid email argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid email</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid email argument</returns>
        public static DomainArgumentException ArgumentIsInvalidEmail(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidemail, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid two-letter ISO 3166 country code argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid ISO 3166 country code</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid ISO 6391 language code argument</returns>
        public static DomainArgumentException ArgumentIsInvalidIso3166CountryCode(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidcountrycode, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid ISO 6391 language code argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid ISO 6391 language code</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid ISO 6391 language code argument</returns>
        public static DomainArgumentException ArgumentIsInvalidIso6391LanguageCode(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidiso6391code, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid ISO 4217 currency code argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid ISO 4217 currency code</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid ISO 4217 currency code argument</returns>
        public static DomainArgumentException ArgumentIsInvalidIso4217CurrencyCode(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidiso4217code, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid phone number argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid address</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid phone number argument</returns>
        public static DomainArgumentException ArgumentIsInvalidPhoneNumber(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidphonenumber, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid phone number prefix argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid phone number prefix</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid phone number prefix argument</returns>
        public static DomainArgumentException ArgumentIsInvalidPhoneNumberPrefix(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidphonenumberprefix, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid time zone id argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid time zone id</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid time zone id argument</returns>
        public static DomainArgumentException ArgumentIsInvalidTimeZoneId(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvalidtimezoneid, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an invalid uri argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="value">The invalid uri</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an invalid uri argument</returns>
        public static DomainArgumentException ArgumentIsInvalidUri(string argumentName, string value)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisinvaliduri, argumentName, value), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when an unsupported value has been supplied for the specified argument
        /// </summary>
        /// <param name="argumentName">The name of the argument that has an unsupported value</param>
        /// <param name="value">The unsupported value</param>
        /// <param name="supportedValues">An array containing the supported values</param>
        /// <returns>A new <see cref="DomainException"/> thrown when an unsupported value has been supplied for the specified argument</returns>
        public static DomainException ArgumentIsUnsupportedValue(string argumentName, object value, params object[] supportedValues)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentisunsupportedvalue, argumentName, value, string.Join(", ", supportedValues)), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> thrown when an argument must be higher or lower than a specific value
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher or lower than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> thrown when an argument must be higher or lower than a specific value</returns>
        public static DomainArgumentException ArgumentMustBeHigherOrLowerThan(string argumentName, int quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherorlowerthan, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> thrown when an argument must be higher or lower than a specific value
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher or lower than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> thrown when an argument must be higher or lower than a specific value</returns>
        public static DomainArgumentException ArgumentMustBeHigherOrLowerThan(string argumentName, double quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherorlowerthan, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> thrown when an argument must be higher or lower than a specific value
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher or lower than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> thrown when an argument must be higher or lower than a specific value</returns>
        public static DomainArgumentException ArgumentMustBeHigherOrLowerThan(string argumentName, decimal quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherorlowerthan, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be lower or higher than the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be lower or higher than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be lower or higher than the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeLowerOrHigherThan(string argumentName, int quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbelowerorhigherthan, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeHigherThan(string argumentName, int quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherthan, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeHigherThan(string argumentName, double quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherthan, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeHigherThan(string argumentName, decimal quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherthan, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher or equal to the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher or equal to</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher or equal to the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeHigherOrEqualTo(string argumentName, int quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherorequalto, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher or equal to the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher or equal to</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher or equal to the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeHigherOrEqualTo(string argumentName, double quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherorequalto, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher or equal to the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be higher or equal to</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher or equal to the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeHigherOrEqualTo(string argumentName, decimal quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbehigherorequalto, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be lower or equal to the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be lower or equal to</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be lower or equal to the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeLowerOrEqualTo(string argumentName, int quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbelowerorequalto, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be lower or equal to the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be lower or equal to</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be lower or equal to the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeLowerOrEqualTo(string argumentName, double quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbelowerorequalto, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be lower or equal to the specified quantity
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="quantity">The quantity the argument must be lower or equal to</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be lower or equal to the specified quantity</returns>
        public static DomainArgumentException ArgumentMustBeLowerOrEqualTo(string argumentName, decimal quantity)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbelowerorequalto, argumentName, quantity), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be lower than the specified time
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="time">The time the argument must be lower than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be lower than the specified time</returns>
        public static DomainArgumentException ArgumentMustBeEarlierThan(string argumentName, TimeSpan time)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbeearlierthan, argumentName, time), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified time
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="time">The time the argument must be higher than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified time</returns>
        public static DomainArgumentException ArgumentMustBeLaterThan(string argumentName, TimeSpan time)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbelaterthan, argumentName, time), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified date
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="date">The date the argument must be higher than</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be higher than the specified date</returns>
        public static DomainArgumentException ArgumentMustBeLaterThan(string argumentName, DateTime date)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbelaterthan, argumentName, date), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that doesn't have the minimum required length
        /// </summary>
        /// <param name="argumentName">The name of the argument that does not have the minimum required length</param>
        /// <param name="length">The minimum required length</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that doesn't have the minimum required length</returns>
        public static DomainArgumentException ArgumentMustHaveMinimumLengthOf(string argumentName, int length)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmusthaveminimumlengthof, length), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument has a length higher than the maximum authorized length
        /// </summary>
        /// <param name="argumentName">The name of the argument has a length higher than the maximum authorized length</param>
        /// <param name="length">The minimum required length</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument has a length higher than the maximum authorized length</returns>
        public static DomainArgumentException ArgumentMustHaveMaximumLengthOf(string argumentName, int length)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmusthavemaximumlengthof, length), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for an argument that must be comprised between the specified values
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for an argument that must be comprised between the specified values</returns>
        public static DomainArgumentException ArgumentMustBeComprisedBetweenIncluding(string argumentName, int min, int max)
        {
            return new DomainArgumentException(StringExtensions.Format(Resources.exception_domain_argumentmustbecomprisedbetweenincluding, argumentName, min, max), argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainArgumentException"/> for a non-numeric argument
        /// </summary>
        /// <param name="argumentName">The argument's name</param>
        /// <returns>A new <see cref="DomainArgumentException"/> for a non-numeric argument</returns>
        public static DomainArgumentException ArgumentMustBeNumeric(string argumentName)
        {
            return new DomainArgumentException(Resources.exception_domain_argumentmustbenumeric, argumentName);
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when deletion of the specified entity type is not supported
        /// </summary>
        /// <param name="entityType">The type of entity for which deletion is not supported</param>
        /// <returns>A new <see cref="DomainException"/> thrown when deletion of the specified entity type is not supported</returns>
        public static DomainException DeletionNotSupported(Type entityType)
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_deletionnotsupported, entityType.Name));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> fired whenever an entity of the specified type already exists
        /// </summary>
        /// <param name="entityType">The type of entity that already exists</param>
        /// <param name="key">The key of the entity that already exists</param>
        /// <param name="keyProperty">The name of the entity's key property</param>
        /// <returns>A new <see cref="DomainException"/> fired whenever an entity of the specified type already exists</returns>
        public static DomainException EntityAlreadyExists(Type entityType, object key, string keyProperty = "id")
        {
            return EntityAlreadyExists(entityType.Name, key, keyProperty);
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> fired whenever an entity of the specified type already exists
        /// </summary>
        /// <param name="entityTypeName">The name of the type of entity that already exists</param>
        /// <param name="key">The key of the entity that already exists</param>
        /// <param name="keyProperty">The name of the entity's key property</param>
        /// <returns>A new <see cref="DomainException"/> fired whenever an entity of the specified type already exists</returns>
        public static DomainException EntityAlreadyExists(string entityTypeName, object key, string keyProperty = "id")
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_entityalreadyexists, entityTypeName, keyProperty.Slugify(" ", true), key));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> fired whenever an entity of the specified type already exists
        /// </summary>
        /// <param name="entityType">The type of entity that already exists</param>
        /// <param name="properties">An array containing the names of the properties of the entity that already exists</param>
        /// <returns>A new <see cref="DomainException"/> fired whenever an entity of the specified type already exists</returns>
        public static DomainException EntityAlreadyExists(Type entityType, string[] properties)
        {
            return EntityAlreadyExists(entityType.Name, properties);
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> fired whenever an entity of the specified type already exists
        /// </summary>
        /// <param name="entityTypeName">The name of the type of entity that already exists</param>
        /// <param name="properties">An array containing the names of the properties of the entity that already exists</param>
        /// <returns>A new <see cref="DomainException"/> fired whenever an entity of the specified type already exists</returns>
        public static DomainException EntityAlreadyExists(string entityTypeName, params string[] properties)
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_entityalreadyexists_many, entityTypeName, string.Join(", ", properties.Select(p => p.Slugify(" ", true)))));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when filtering is not allowed for the specified entity type
        /// </summary>
        /// <param name="entityType">The entity type for which filtering is not allow</param>
        /// <returns>A new <see cref="DomainException"/></returns>
        public static DomainException FilteringNotAllowed(Type entityType)
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_filteringnotallowed, entityType.Name));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when an invalid cast occured
        /// </summary>
        /// <param name="from">The type from which the cast occured</param>
        /// <param name="to">The type to which the cast occured</param>
        /// <returns>A new <see cref="DomainException"/></returns>
        public static DomainException InvalidCast(Type from, Type to)
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_invalidcast, from.Name, to.Name));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when a property cannot be found on a type
        /// </summary>
        /// <param name="type">The type that is missing the specified property</param>
        /// <param name="property">The property that is missing from the specified type</param>
        /// <returns>A new <see cref="DomainException"/></returns>
        public static DomainException MissingProperty(Type type, string property)
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_missingproperty, property, type.Name));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when an unsupported value has been supplied for the specified type
        /// </summary>
        /// <param name="type">The type for which an unsupported value has been supplied</param>
        /// <param name="value">The unsupported value</param>
        /// <returns>A new <see cref="DomainException"/> thrown when an unsupported value has been supplied for the specified type</returns>
        public static DomainException NotSupported(Type type, object value)
        {
            return NotSupported(type.Name, value);
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when an unsupported value has been supplied for the specified type
        /// </summary>
        /// <param name="typeName">The name of the type for which an unsupported value has been supplied</param>
        /// <param name="value">The unsupported value</param>
        /// <returns>A new <see cref="DomainException"/> thrown when an unsupported value has been supplied for the specified type</returns>
        public static DomainException NotSupported(string typeName, object value)
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_notsupported, typeName, value));
        }

        /// <summary>
        /// Creates a new <see cref="DomainNullReferenceException"/> thrown when an entity of the specified type has not been found
        /// </summary>
        /// <param name="entityType">The type of the entity that could not be found</param>
        /// <param name="key">The key of the entity that could not be found</param>
        /// <param name="keyProperty">The name of the key property</param>
        /// <returns>A new <see cref="DomainException"/> thrown when an entity of the specified type has not been found</returns>
        public static DomainNullReferenceException NullReference(Type entityType, object key, string keyProperty = "id")
        {
            return NullReference(entityType.Name, key, keyProperty);
        }

        /// <summary>
        /// Creates a new <see cref="DomainNullReferenceException"/> thrown when an entity of the specified type has not been found
        /// </summary>
        /// <param name="entityTypeName">The name of the type of the entity that could not be found</param>
        /// <param name="key">The key of the entity that could not be found</param>
        /// <param name="keyProperty">The name of the key property</param>
        /// <returns>A new <see cref="DomainException"/> thrown when an entity of the specified type has not been found</returns>
        public static DomainNullReferenceException NullReference(string entityTypeName, object key, string keyProperty = "id")
        {
            return new DomainNullReferenceException(StringExtensions.Format(Resources.exception_domain_nullreference, entityTypeName, keyProperty.Slugify(" ", true), key));
        }

        /// <summary>
        /// Creates a new <see cref="DomainNullReferenceException"/> thrown when an entity of the specified type has not been found
        /// </summary>
        /// <param name="entityType">The type of the entity that could not be found</param>
        /// <param name="keyValues">An array containing the name/value pairs of the entity's keys</param>
        /// <returns>A new <see cref="DomainException"/> thrown when an entity of the specified type has not been found</returns>
        public static DomainNullReferenceException NullReference(Type entityType, params KeyValuePair<string, object>[] keyValues)
        {
            return NullReference(entityType.Name, keyValues);
        }

        /// <summary>
        /// Creates a new <see cref="DomainNullReferenceException"/> thrown when an entity of the specified type has not been found
        /// </summary>
        /// <param name="entityTypeName">The name of the type of the entity that could not be found</param>
        /// <param name="keyValues">An array containing the name/value pairs of the entity's keys</param>
        /// <returns>A new <see cref="DomainException"/> thrown when an entity of the specified type has not been found</returns>
        public static DomainNullReferenceException NullReference(string entityTypeName, params KeyValuePair<string, object>[] keyValues)
        {
            return new DomainNullReferenceException(StringExtensions.Format(Resources.exception_domain_nullreference_many, entityTypeName, Environment.NewLine + string.Join(Environment.NewLine, keyValues.Select(kvp => $"{kvp.Key}: '{kvp.Value}'"))));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when an entity is in an unexpected state
        /// </summary>
        /// <param name="entityType">The type of entity that is in an unexpected state</param>
        /// <param name="key">The key of the entity that is in an unexpected state</param>
        /// <param name="state">The entity's state</param>
        /// <param name="keyProperty">The name of the entity's key property. Defaults to 'id'</param>
        /// <returns>A new <see cref="DomainException"/></returns>
        public static DomainException UnexpectedState(Type entityType, object key, object state, string keyProperty = "id")
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_unexpectedstate, entityType.Name, keyProperty, key.ToString(), state.ToString()));
        }

        /// <summary>
        /// Creates a new <see cref="DomainException"/> thrown when an entity has an unexpected value
        /// </summary>
        /// <param name="entityType">The type of entity that has an unexpected value</param>
        /// <param name="key">The key of the entity that is in an unexpected state</param>
        /// <param name="value">The entity's value</param>
        /// <param name="valueProperty">The name of the entity's property that has an unexpected value</param>
        /// <param name="keyProperty">The name of the entity's key property. Defaults to 'id'</param>
        /// <returns>A new <see cref="DomainException"/></returns>
        public static DomainException UnexpectedValue(Type entityType, object key, object value, string valueProperty, string keyProperty = "id")
        {
            return new DomainException(StringExtensions.Format(Resources.exception_domain_unexpectedvalue, entityType.Name, keyProperty, key.ToString(), valueProperty, value.ToString()));
        }

    }

}
