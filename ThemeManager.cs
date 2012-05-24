using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Maestro;


namespace WPF.Themes
{    
    public static class ThemeManager
    {
        private static SqlConnection conn;
        private static string connString = Properties.Settings.Default.connStringMaestro;
        private static byte _bId_App;
        
        public static ResourceDictionary GetThemeResourceDictionary(string theme)
        {
            if (theme != null)
            {
                Assembly assembly = Assembly.LoadFrom("WPF.Themes.dll");
                string packUri = String.Format(@"/WPF.Themes;component/{0}/Theme.xaml", theme);
                return Application.LoadComponent(new Uri(packUri, UriKind.Relative)) as ResourceDictionary;
            }
            return null;
        }

        public static string[] GetThemes()
        { 
            int i = 0;
            using (conn = new SqlConnection(connString))
            {
                conn.Open();
                ObservableCollection<Temas> temas = new Temas(conn).TraerTodos();

                string[] themes = new string[temas.Count];
                foreach (Temas tema in temas)
                {
                    themes[i++] = tema.Nombre;
                }
                return themes;
            }

            //string[] themes = new string[] 
            //{ 
            //    "Default", //Sin theme
            //    "ExpressionDark", "ExpressionLight", 
            //    //"RainierOrange", "RainierPurple", "RainierRadialBlue", 
            //    "ShinyBlue", "ShinyRed", 
            //    //"ShinyDarkTeal", "ShinyDarkGreen", "ShinyDarkPurple",
            //    "DavesGlossyControls", 
            //    "WhistlerBlue", 
            //    "BureauBlack", "BureauBlue", 
            //    "BubbleCreme", 
            //    "TwilightBlue",
            //    "UXMusingsRed", "UXMusingsGreen", 
            //    //"UXMusingsRoughRed", "UXMusingsRoughGreen", 
            //    "UXMusingsBubblyBlue"
            //};
            //return themes;
        }

        public static void ApplyTheme(this Application app, string theme, byte bId_App)
        {
            _bId_App = bId_App;
            ResourceDictionary dictionary = ThemeManager.GetThemeResourceDictionary(theme);
            ResourceDictionary dictionaryparatabs = ThemeManager.GetThemeResourceDictionary("Default");
            if (dictionary != null)
            {
                
                app.Resources.MergedDictionaries.Clear();
                app.Resources.MergedDictionaries.Add(dictionary);
                app.Resources.MergedDictionaries.Add(dictionaryparatabs);

                Guardar_Tema(theme, bId_App);


            }
        }

        public static void ApplyTheme(this ContentControl control, string theme,byte bId_App)
        {
            _bId_App = bId_App;
            ResourceDictionary dictionary = ThemeManager.GetThemeResourceDictionary(theme);
            ResourceDictionary dictionaryparatabs = ThemeManager.GetThemeResourceDictionary("Default");

            if (dictionary != null)
            {
                control.Resources.MergedDictionaries.Clear();
                control.Resources.MergedDictionaries.Add(dictionary);
                control.Resources.MergedDictionaries.Add(dictionaryparatabs);

                Guardar_Tema(theme, bId_App);
            }
        }
        
        public static void Guardar_Tema(string sTheme, byte bId_App)
        {
            _bId_App = bId_App;
            try
            {
                using (conn = new SqlConnection(connString))
                {
                    conn.Open();
                    var Id_Tema = (from c in new Temas(conn).TraerPor_Nombre(sTheme, "T") select c.Id_Tema).FirstOrDefault();
                    
                    new Temas_Aplicaciones_Usuarios(conn).Modificar(Environment.UserName, bId_App, Id_Tema  );
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string Leer_Tema(byte bId_App)
        {
            Temas_Aplicaciones_Usuarios temasAplicacionesUsuarios =  null;
            _bId_App = bId_App;
            try
            {
                using (conn = new SqlConnection(connString))
                {
                    conn.Open();
                    temasAplicacionesUsuarios = new Temas_Aplicaciones_Usuarios(conn);

                    if(temasAplicacionesUsuarios.TraerFilaTotal(Environment.UserName,bId_App) == false)
                    {
                        return "Default";
                    }
                }
                return temasAplicacionesUsuarios.Tema.Nombre;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Theme

        /// <summary>
        /// Theme Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.RegisterAttached("Theme", typeof(string), typeof(ThemeManager),
                new FrameworkPropertyMetadata((string)string.Empty,
                    new PropertyChangedCallback(OnThemeChanged)));

        /// <summary>
        /// Gets the Theme property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static string GetTheme(DependencyObject d)
        {
            return (string)d.GetValue(ThemeProperty);
        }

        /// <summary>
        /// Sets the Theme property.  This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetTheme(DependencyObject d, string value)
        {
            d.SetValue(ThemeProperty, value);
        }

        /// <summary>
        /// Handles changes to the Theme property.
        /// </summary>
        private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string theme = e.NewValue as string;
            if (theme == string.Empty)
                return;

            ContentControl control = d as ContentControl;
            if (control != null)
            {
                control.ApplyTheme(theme,_bId_App);
            }
        }

   
        

        #endregion
    }
}
