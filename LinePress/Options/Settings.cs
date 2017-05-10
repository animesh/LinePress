using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace LinePress.Options
{
   public class LinePressSettings : ISettings, INotifyPropertyChanged
   {
      #region Fields
      private bool compressEmptyLines = true;
      private bool compressCustomTokens = true;

      private double emptyLineRate = 0.5;
      private double customTokensRate = 0.75;

      private string customTokensString = string.Empty;
      private ObservableCollection<string> customTokens;
      #endregion

      public LinePressSettings()
      {
         CustomTokens = new ObservableCollection<string> { "{", "}" };

         InsertTokenCommand = new RelayCommand<string>(CanInsertToken, t => CustomTokens.Add(t));
         DeleteTokenCommand = new RelayCommand<string>(CanDeleteToken, t => CustomTokens.Remove(t));
      }

      public RelayCommand<string> InsertTokenCommand { get; private set; }
      public RelayCommand<string> DeleteTokenCommand { get; private set; }

      public ObservableCollection<string> CustomTokens
      {
         get { return customTokens; }
         private set
         {
            SetField(ref customTokens, value);
            customTokens.CollectionChanged += (o, e) => SyncCustomTokensString();
         }
      }

      #region Settings to Save

      [Setting]
      public bool FirstRun { get; set; } = true;

      [Setting]
      public bool CompressEmptyLines
      {
         get { return compressEmptyLines; }
         set { SetField(ref compressEmptyLines, value); }
      }

      [Setting]
      public double EmptyLineScale
      {
         get { return emptyLineRate; }
         set { SetField(ref emptyLineRate, value); }
      }

      [Setting]
      public bool CompressCustomTokens
      {
         get { return compressCustomTokens; }
         set { SetField(ref compressCustomTokens, value); }
      }

      [Setting]
      public double CustomTokensScale
      {
         get { return customTokensRate; }
         set { SetField(ref customTokensRate, value); }
      }

      [Setting]
      public string CustomTokensString
      {
         get { return customTokensString; }
         set
         {
            SetField(ref customTokensString, value);
            SyncCustomTokensList();
         }
      }

      #endregion
      
      #region Commands Predicates

      private bool CanInsertToken(string token) =>
         !string.IsNullOrWhiteSpace(token) && !CustomTokens.Contains(token);

      private bool CanDeleteToken(string token) =>
         !string.IsNullOrWhiteSpace(token) && CustomTokens.Contains(token);

      #endregion

      #region Helpers

      private void SyncCustomTokensList()
      {
         CustomTokens = new ObservableCollection<string>(customTokensString.Split(null));
      }

      private void SyncCustomTokensString()
      {
         var stringBuilder = new StringBuilder(customTokens[0]);
         
         for (var i = 1; i < customTokens.Count; i++)
            stringBuilder.Append($" {customTokens[i]}");

         customTokensString = stringBuilder.ToString();
      }

      #endregion

      #region ISettings Members

      public string Name => "LinePress";

      #endregion

      #region INotifyPropertyChanged Members

      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
      {
         if (EqualityComparer<T>.Default.Equals(field, value))
            return;

         field = value;

         OnPropertyChanged(propertyName);
      }
      #endregion
   }
}
