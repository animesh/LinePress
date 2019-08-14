using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace LinePress.Options
{
   public static class SettingsStore
   {
      private static readonly WritableSettingsStore store
          = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetWritableSettingsStore(SettingsScope.UserSettings);

      public static event Action SettingsChanged;

      public static void SaveSettings(ISettings settings)
      {
         try
         {
            if (!store.CollectionExists(settings.Key))
               store.CreateCollection(settings.Key);

            if (SaveSettingsIntoStore(settings))
               SettingsChanged?.Invoke();
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }

      public static void LoadSettings(ISettings settings)
      {
         try
         {
            if (store.CollectionExists(settings.Key))
               LoadSettingsFromStore(settings);
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }

      private static bool SaveSettingsIntoStore(ISettings settings)
      {
         var isSaved = false;

         foreach (var prop in GetProperties(settings))
         {
            switch (prop.GetValue(settings))
            {
               case bool b:
                  store.SetBoolean(settings.Key, prop.Name, b);
                  isSaved = true;
                  break;

               case int i:
                  store.SetInt32(settings.Key, prop.Name, i);
                  isSaved = true;
                  break;

               case double d:
                  store.SetString(settings.Key, prop.Name, d.ToString());
                  isSaved = true;
                  break;

               case string s:
                  store.SetString(settings.Key, prop.Name, s);
                  isSaved = true;
                  break;
            }
         }

         return isSaved;
      }

      private static void LoadSettingsFromStore(ISettings settings)
      {
         foreach (var prop in GetProperties(settings))
         {
            if (store.PropertyExists(settings.Key, prop.Name))
            {
               switch (prop.GetValue(settings))
               {
                  case bool b:
                     prop.SetValue(settings, store.GetBoolean(settings.Key, prop.Name));
                     break;

                  case int i:
                     prop.SetValue(settings, store.GetInt32(settings.Key, prop.Name));
                     break;

                  case double d when double.TryParse(store.GetString(settings.Key, prop.Name), out double value):
                     prop.SetValue(settings, value);
                     break;

                  case string s:
                     prop.SetValue(settings, store.GetString(settings.Key, prop.Name));
                     break;
               }
            }
         }
      }

      private static IEnumerable<PropertyInfo> GetProperties(ISettings settings)
      {
         return settings.GetType()
                        .GetProperties()
                        .Where(p => Attribute.IsDefined(p, typeof(SettingAttribute)));
      }
   }
}