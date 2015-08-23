﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class ModController : MonoBehaviour
    {

        #region Instance Data

        public readonly string              GameObjectName = "Community Core Library";

        public static List< AdvancedResearchDef > AdvancedResearch;

        public static Version               CCLVersion;
        List< ModHelperDef >                ModHelperDefs;

        #endregion

        #region Mono Callbacks

        public void                         Start()
        {
            enabled = false;

            // Check versions of mods and throw error to the user if the
            // mod version requirement is higher than the installed version
            GetCCLVersion();

            ModHelperDefs = DefDatabase< ModHelperDef >.AllDefs.ToList();

            if( ( ModHelperDefs != null )&&
                ( ModHelperDefs.Count > 0 ) )
            {
                ValidateMods();
            }

            // Validate advanced research defs
            CheckAdvancedResearch();

            Log.Message( "Community Core Library :: Initialized" );

            enabled = true;
        }

        public void                         FixedUpdate()
        {
            if( ReadyForDesignatorInjection() )
            {
                InjectDesignators();
            }
            if( ReadyForMapComponentInjection() )
            {
                InjectMapComponents();
            }
        }

	    public void							OnLevelWasLoaded()
	    {
		    InjectThingComp<CompPawnGizmo>("Human");
	    }

        #endregion

        #region Versioning

        void                                GetCCLVersion ()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            CCLVersion = assembly.GetName().Version;
#if DEBUG
            Log.Message( "Community Core Library v" + CCLVersion + " (debug)" );
#else
            Log.Message( "Community Core Library v" + CCLVersion );
#endif
        }

        #endregion

        #region Mod Validation

        void                                ValidateMods()
        {
            for( int i = 0; i < ModHelperDefs.Count; i++ ){
                var ModHelperDef = ModHelperDefs[ i ];
                if( !ModHelperDef.IsValid )
                {
                    // Don't do anything special with broken mods
                    ModHelperDefs.Remove( ModHelperDef );
                    continue;
                }
            }
        }

        #endregion

        #region Research Verification

        void                                CheckAdvancedResearch()
        {
            // Make sure the hidden research exists
            if( Research.Locker == null )
            {
                Log.Error( "Community Core Library :: Advanced Research :: Missing Research.Locker!" );
            }

            // Get the [advanced] research defs
            AdvancedResearch = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();

            if( ( AdvancedResearch == null )&&
                ( AdvancedResearch.Count == 0 ) )
            {
                return;
            }

            // Validate each advanced research def
            for( int i = 0; i < AdvancedResearch.Count; i++ ){
                var Advanced = AdvancedResearch[ i ];

                if( !Advanced.IsValid )
                {
                    // Remove projects with errors from list of usable projects
                    AdvancedResearch.Remove( Advanced );
                    continue;
                }
            }

            // All advanced research checked, no log errors means it's all good
        }

        #endregion

        #region Map Component Injection

        bool                                ReadyForMapComponentInjection()
        {
            return (
                ( Find.Map != null )&&
                ( Find.Map.components != null )
            );
        }

        void                                InjectMapComponents()
        {
            // Inject the map components into the game
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.MapComponentsInjected )
                {
                    Log.Message( "Community Core Library :: Injecting MapComponents for " + ModHelperDef.ModName );
                    ModHelperDef.InjectMapComponents();
                }
            }
        }

        #endregion

        #region Designator Injection

        bool                                ReadyForDesignatorInjection()
        {
            var DesignatorCategoryDefs = DefDatabase<DesignationCategoryDef>.AllDefs;

            foreach( var DesignatorCategoryDef in DesignatorCategoryDefs )
            {
                if( DesignatorCategoryDef.resolvedDesignators == null )
                {
                    return false;
                }
            }

            return true;
        }

        void                                InjectDesignators()
        {
            // Inject the designators into the categories
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.DesignatorsInjected )
                {
                    Log.Message( "Community Core Library :: Injecting Designators for " + ModHelperDef.ModName );
                    ModHelperDef.InjectDesignators();
                }
            }
        }

		#endregion

		#region ThingComp Injection

	    void								InjectThingComp<T>(string defName) where T : ThingComp
		{
			// Access ThingDef database
			var typeFromHandle = typeof(DefDatabase<ThingDef>);
			var defsByName = typeFromHandle.GetField("defsByName", BindingFlags.Static | BindingFlags.NonPublic);
			if (defsByName == null)
			{
				CCL_Log.Error("ThingComp Injection", "defName is null!");
			}
			var dictDefsByName = defsByName?.GetValue(null) as Dictionary<string, ThingDef>;
			if (dictDefsByName == null)
			{
				CCL_Log.Error("ThingComp Injection", "Cannot access private members!");
			}

			var def = dictDefsByName?.Values.ToList().Find(s => s.defName == defName);

			if (def != null && def.comps.Any(curComp => curComp.GetType() == typeof(T)))
			{
				return;
			}

			CCL_Log.Message("ThingComp Injection", "Injecting ThingComp to" + def.defName);
			var compProperties = new CompProperties { compClass = typeof(T) };
			def.comps.Add(compProperties);
		}

		#endregion

	}

}
