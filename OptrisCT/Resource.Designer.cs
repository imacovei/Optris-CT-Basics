﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OptrisCT {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OptrisCT.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot open the COM port. {0}.
        /// </summary>
        internal static string ERR_CANNOT_OPEN_COM_PORT {
            get {
                return ResourceManager.GetString("ERR_CANNOT_OPEN_COM_PORT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid value {0}, list of OPTRIS-CT addresses is empty.
        /// </summary>
        internal static string ERR_EMPTY_ADDRESS_LIST {
            get {
                return ResourceManager.GetString("ERR_EMPTY_ADDRESS_LIST", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No COM port provided.
        /// </summary>
        internal static string ERR_EMPTY_COM_PORT {
            get {
                return ResourceManager.GetString("ERR_EMPTY_COM_PORT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided parameters are inconsistent. The count of temperature correction values must be equal to the count of addresses.
        /// </summary>
        internal static string ERR_INCONSISTENT_PARAMATERS_COUNT {
            get {
                return ResourceManager.GetString("ERR_INCONSISTENT_PARAMATERS_COUNT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided parameters are inconsistent. Each address must have a temperature correction value.
        /// </summary>
        internal static string ERR_INCONSISTENT_PARAMETERS_MISSING {
            get {
                return ResourceManager.GetString("ERR_INCONSISTENT_PARAMETERS_MISSING", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid value for OPTRIS-CT address. Min: 1, max: {0}. Provided: {1}.
        /// </summary>
        internal static string ERR_INVALID_ADDRESS_IN_LIST {
            get {
                return ResourceManager.GetString("ERR_INVALID_ADDRESS_IN_LIST", resourceCulture);
            }
        }
    }
}
