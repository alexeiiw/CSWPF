using MahApps.Metro.Controls;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ActualizarDatosEquipo
{
    // Define clases
    public class AutorizaSISCON
    {
        public string Usuario { get; set; }
        public string CorreoElectronico { get; set; }
        public AutorizaSISCON(string usuario, string correoelectronico)
        {
            Usuario = usuario;
            CorreoElectronico = correoelectronico;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // Define objetos globales
        DataTable dtDatosAnterior = new DataTable();

        string connStringSISCON = "data source=128.1.200.167;initial catalog=Canella_SISCON;persist security info=True;user id=usrsap;password=C@nella20$";

        int intIdTransaccion = 0;

        bool Logueado = false;

        AutorizaSISCON objAutorizaSISCON = new AutorizaSISCON("", "");

        string strSerie = "3ukrR6F5";

        int Departamento = 0; // 13 Servicio Tecnico, 2 Soluciones

        string SconConexion = "[128.1.5.79].scon.dbo."; // produccion
        //string SconConexion = "Scon_06062021.dbo."; // desarrollo

        public MainWindow()
        {
            InitializeComponent();
        }

        // Cuando se inicia la forma
        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // Valida que el número de serie de la aplicación sea válido
            if (Validar_SerieAPP()) { Iniciar_Pantalla(); }
            else
            {
                MessageBox.Show("Su versión de aplicación ya no es válida, favor comunicarse con el Administrador!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }

            // Obtiene el rol autorizar de SISCON
            Obtener_UsuarioAutorizadorSISCON();
        }

        // Obtiene el usuario que autoriza de SISCON
        private void Obtener_UsuarioAutorizadorSISCON()
        {
            string strSql = "select * from DEF_USUARIOS where gerente = '1' and cod_departamento = 2";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                // Obtiene los valores del usuario autorizador de SISCON    
                objAutorizaSISCON.Usuario = dt.Rows[0]["cod_usuario"].ToString();
                objAutorizaSISCON.CorreoElectronico = dt.Rows[0]["correo_electronico"].ToString();
            }

        }

        // Metodo que valida la serie y la vigencia de la aplicacion
        private bool Validar_SerieAPP()
        {
            string strSql = "select * from UTILS.dbo.SeriesAplicaciones where Serie = '" + strSerie + "'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                if (bool.Parse(dt.Rows[0]["Activo"].ToString()))
                {
                    Title = dt.Rows[0]["Aplicacion"].ToString() + " Versión: " + dt.Rows[0]["Version"].ToString() + " " + dt.Rows[0]["Ambiente"].ToString();
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        // Valida que solo se ingrese números
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Metodo que se utilizara al iniciar el formulario
        private void Iniciar_Pantalla()
        {
            // Llena los pendientes
            Obtener_Pendientes();
        }

        // Limpia el formulario para una nueva busqueda
        private void Limpiar_Formulario()
        {
            // Limpia el formulario
            txtSerie.Text = "";
            txtNombreImpresora.Text = "";
            txtMacAddress.Text = "";
            txtDireccionIp.Text = "";
            txtIdCliente.Text = "";
            txtNumeroAgencia.Text = "";
            txtCentroComercial.Text = "";
            txtArea.Text = "";
            txtZonaGeografica.Text = "";
            txtAgenciaDepto.Text = "";
            txtEncargado.Text = "";
            txtCuotaFija.Text = "";
            txtTelefonoEncargado.Text = "";
            txtVolumenBn.Text = "";
            txtDireccion.Text = "";
            txtVolumenColor.Text = "";
            txtNivel.Text = "";
            txtComentarios.Text = "";
            lblContrato.Content = "";
            lblCliente.Content = "";

            // Formulario de dirección
            cmbDepartamento.SelectedIndex = 0;
            cmbCiudad.SelectedIndex = 0;
            txtDireccion1.Text = "";
            txtDireccion2.Text = "";
            txtZona.Text = "";

            // Deshabilita los Tabs y botones
            tabHistorico.IsEnabled = false;
            btnActualizar.IsEnabled = false;
            btnCancelar.IsEnabled = false;
            tabDireccion.IsEnabled = false;
            tabPendientes.IsEnabled = false;

            // Deshabilita los textbox
            txtNombreImpresora.IsEnabled = false;
            txtMacAddress.IsEnabled = false;
            txtDireccionIp.IsEnabled = false;
            txtIdCliente.IsEnabled = false;
            txtNumeroAgencia.IsEnabled = false;
            txtCentroComercial.IsEnabled = false;
            txtArea.IsEnabled = false;
            txtZonaGeografica.IsEnabled = false;
            txtAgenciaDepto.IsEnabled = false;
            txtEncargado.IsEnabled = false;
            txtTelefonoEncargado.IsEnabled = false;
            txtComentarios.IsEnabled = false;
            txtDireccion.IsEnabled = false;
            btnObtenerDireccion.IsEnabled = false;
            txtVolumenColor.IsEnabled = false;
            txtVolumenBn.IsEnabled = false;
            txtCuotaFija.IsEnabled = false;
            txtNivel.IsEnabled = false;

            // Oculta los labels de bitacora
            lblAprobado.Visibility = Visibility.Hidden;
            lblFechaActual.Visibility = Visibility.Hidden;
            lblFechaRegistro.Visibility = Visibility.Hidden;
            lblSistema.Visibility = Visibility.Hidden;
            lblComentarios.Visibility = Visibility.Hidden;
            lblUsuario.Visibility = Visibility.Hidden;
            lblTransaccion.Visibility = Visibility.Hidden;

            // Coloca el foco en el tab de datos
            tabDatos.Focus();
        }

        // Metodo para generar el cuerpo del correo
        private string Generar_CuerpoCorreo()
        {
            string strCuerpo = "";

            strCuerpo += "<table width=\"425px\"><tr><td><font size=3 face=Calibri>Se ha generado una solicitud de actualización de datos para la serie: </font><font face=Calibri size=5><b>" + txtSerie.Text + "</b></font></td></tr>";
            strCuerpo += "<tr><td><font size=3 face=Calibri>Favor ingresar a la aplicación para autorizar y actualizar los datos.</font></td></tr><br/><tr>";

            strCuerpo += "<td><font size=3 face=Calibri><b>Datos Anteriores:";
            strCuerpo += "</b></font></td></tr><tr><td style=\"border: 1px solid\"><font size=3 face=Calibri>";
            strCuerpo += "Transacción: " + intIdTransaccion.ToString() + "<br>";
            strCuerpo += "Nombre Impresora: " + dtDatosAnterior.Rows[0]["nombre_equipo"].ToString() + "<br>";
            strCuerpo += "Dirección: " + dtDatosAnterior.Rows[0]["direccion_equi_empresa"].ToString() + "<br>";
            strCuerpo += "Agencia-Depto.: " + dtDatosAnterior.Rows[0]["agencia_depto"].ToString() + "<br>";
            strCuerpo += "</font></td></tr><br/>";

            strCuerpo += "<td><font size=3 face=Calibri><b>Datos Nuevos:";
            strCuerpo += "</b></font></td></tr><tr><td style=\"border: 1px solid\"><font size=3 face=Calibri>";
            strCuerpo += "Transacción: " + intIdTransaccion.ToString() + "<br>";
            strCuerpo += "Nombre Impresora: " + txtNombreImpresora.Text + "<br>";
            strCuerpo += "Dirección: " + txtDireccion.Text + "<br>";
            strCuerpo += "Agencia-Depto.: " + txtAgenciaDepto.Text + "<br>";
            strCuerpo += "Comentarios: " + txtComentarios.Text + "<br>";
            strCuerpo += "</font></td></tr><br/>";

            return strCuerpo;
        }

        // Metodo para enviar correos electrónicos
        private void Enviar_CorreoInterno(string ToMail, string SubjectMail, string BodyMail)
        {
            string ServidorCorreoCanella = "srv-ex2010";

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("alertassap@canella.com.gt", "Sistema SISCON - Alerta");
            mail.To.Add(ToMail);
            mail.Subject = SubjectMail;
            mail.Body = BodyMail;
            mail.IsBodyHtml = true;
            SmtpClient smpt = new SmtpClient(ServidorCorreoCanella);
            smpt.Send(mail);
        }

        // Metodo que desencripta la contraseña
        private string EncryptDecrypt(string InString)
        {
            int c1;
            string NewEncryptString;
            int EncryptSeed;
            string EncryptChar;

            NewEncryptString = "";
            //EncryptSeed = System.Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings("ck"));
            EncryptSeed = 5;

            for (c1 = 1; c1 <= Strings.Len(InString); c1++)
            {
                EncryptChar = Strings.Mid(InString, c1, 1);
                EncryptChar = Strings.Chr(Strings.Asc(EncryptChar) ^ EncryptSeed).ToString();
                EncryptSeed = EncryptSeed ^ c1;
                NewEncryptString = NewEncryptString + EncryptChar;
            }

            return NewEncryptString;
        }

        // Metodo para validar las credenciales
        private bool Validar_Credenciales()
        {
            string strSql = "select * from DEF_USUARIOS where COD_USUARIO = '" + txtUsuario.Text + "'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["contrasena"].ToString() == EncryptDecrypt(txtClave.Password))
                {
                    // Asigna el departamento
                    Departamento = Convert.ToInt32(dt.Rows[0]["cod_departamento"].ToString());

                    // Coloca el nombre del usuario y el area
                    lblnombreusuario.Content = dt.Rows[0]["nombre_completo"].ToString();
                    lblnombreusuario.Visibility = Visibility.Visible;
                    switch (Departamento)
                    {
                        case 2:
                            lblareausuario.Content = "Soluciones Digitales";
                            break;
                        case 13:
                            lblareausuario.Content = "Servicio Técnico";
                            break;
                    }
                    lblareausuario.Visibility = Visibility.Visible;

                    // Si esta logueado retorna verdadero
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        // Obtiene el nuevo número de transacción
        private int Obtener_IdTransaccion()
        {
            string strSql = "select top 1 (TRANSACCION+1) as transaccion from COT_DATOSEQUIPOS_LOG ORDER BY TRANSACCION desc";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                return int.Parse(dt.Rows[0]["transaccion"].ToString());
            }
            else { return 1; }
        }

        // Metodo para obtener todos los cambios pendientes
        private void Obtener_Pendientes()
        {
            // Create the Grid    
            Grid DynamicGrid = new Grid();
            DynamicGrid.Width = 750;
            DynamicGrid.VerticalAlignment = VerticalAlignment.Top;
            DynamicGrid.Background = new SolidColorBrush(Colors.WhiteSmoke);

            // Create Columns    
            ColumnDefinition gridCol1 = new ColumnDefinition();
            gridCol1.Width = GridLength.Auto;
            ColumnDefinition gridCol2 = new ColumnDefinition();
            ColumnDefinition gridCol3 = new ColumnDefinition();
            ColumnDefinition gridCol4 = new ColumnDefinition();
            ColumnDefinition gridCol5 = new ColumnDefinition();
            gridCol5.Width = GridLength.Auto;
            ColumnDefinition gridCol6 = new ColumnDefinition();
            gridCol6.Width = GridLength.Auto;
            ColumnDefinition gridCol7 = new ColumnDefinition();
            gridCol7.Width = GridLength.Auto;
            ColumnDefinition gridCol8 = new ColumnDefinition();
            gridCol8.Width = GridLength.Auto;
            DynamicGrid.ColumnDefinitions.Add(gridCol1);
            DynamicGrid.ColumnDefinitions.Add(gridCol2);
            DynamicGrid.ColumnDefinitions.Add(gridCol3);
            DynamicGrid.ColumnDefinitions.Add(gridCol4);
            DynamicGrid.ColumnDefinitions.Add(gridCol5);
            DynamicGrid.ColumnDefinitions.Add(gridCol6);
            DynamicGrid.ColumnDefinitions.Add(gridCol7);
            DynamicGrid.ColumnDefinitions.Add(gridCol8);

            // Create Rows    
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(25);
            RowDefinition gridRow2 = new RowDefinition();
            gridRow2.Height = new GridLength(25);
            RowDefinition gridRow3 = new RowDefinition();
            gridRow3.Height = new GridLength(25);
            RowDefinition gridRow4 = new RowDefinition();
            gridRow4.Height = new GridLength(25);
            RowDefinition gridRow5 = new RowDefinition();
            gridRow5.Height = new GridLength(25);
            RowDefinition gridRow6 = new RowDefinition();
            gridRow6.Height = new GridLength(25);
            RowDefinition gridRow7 = new RowDefinition();
            gridRow7.Height = new GridLength(25);
            RowDefinition gridRow8 = new RowDefinition();
            gridRow8.Height = new GridLength(25);
            RowDefinition gridRow9 = new RowDefinition();
            gridRow9.Height = new GridLength(25);
            RowDefinition gridRow10 = new RowDefinition();
            gridRow10.Height = new GridLength(25);
            RowDefinition gridRow11 = new RowDefinition();
            gridRow11.Height = new GridLength(25);
            RowDefinition gridRow12 = new RowDefinition();
            gridRow12.Height = new GridLength(25);
            RowDefinition gridRow13 = new RowDefinition();
            gridRow13.Height = new GridLength(25);
            RowDefinition gridRow14 = new RowDefinition();
            gridRow14.Height = new GridLength(25);
            RowDefinition gridRow15 = new RowDefinition();
            gridRow15.Height = new GridLength(25);

            DynamicGrid.RowDefinitions.Add(gridRow1);
            DynamicGrid.RowDefinitions.Add(gridRow2);
            DynamicGrid.RowDefinitions.Add(gridRow3);
            DynamicGrid.RowDefinitions.Add(gridRow4);
            DynamicGrid.RowDefinitions.Add(gridRow5);
            DynamicGrid.RowDefinitions.Add(gridRow6);
            DynamicGrid.RowDefinitions.Add(gridRow7);
            DynamicGrid.RowDefinitions.Add(gridRow8);
            DynamicGrid.RowDefinitions.Add(gridRow9);
            DynamicGrid.RowDefinitions.Add(gridRow10);
            DynamicGrid.RowDefinitions.Add(gridRow11);
            DynamicGrid.RowDefinitions.Add(gridRow12);
            DynamicGrid.RowDefinitions.Add(gridRow13);
            DynamicGrid.RowDefinitions.Add(gridRow14);
            DynamicGrid.RowDefinitions.Add(gridRow15);

            // Add first column header    
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = " # ";
            txtBlock1.FontSize = 13;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock1.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock1, 0);
            Grid.SetColumn(txtBlock1, 0);

            TextBlock txtBlock2 = new TextBlock();
            txtBlock2.Text = "Nombre Impresora";
            txtBlock2.FontSize = 13;
            txtBlock2.FontWeight = FontWeights.Bold;
            txtBlock2.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock2.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock2.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock2, 0);
            Grid.SetColumn(txtBlock2, 1);

            TextBlock txtBlock3 = new TextBlock();
            txtBlock3.Text = "Dirección";
            txtBlock3.FontSize = 13;
            txtBlock3.FontWeight = FontWeights.Bold;
            txtBlock3.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock3.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock3.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock3, 0);
            Grid.SetColumn(txtBlock3, 2);

            TextBlock txtBlock4 = new TextBlock();
            txtBlock4.Text = "Agencia-Depto.";
            txtBlock4.FontSize = 13;
            txtBlock4.FontWeight = FontWeights.Bold;
            txtBlock4.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock4.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock4.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock4, 0);
            Grid.SetColumn(txtBlock4, 3);

            TextBlock txtBlock5 = new TextBlock();
            txtBlock5.Text = "Fecha Registro";
            txtBlock5.FontSize = 13;
            txtBlock5.FontWeight = FontWeights.Bold;
            txtBlock5.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock5.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock5.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock5, 0);
            Grid.SetColumn(txtBlock5, 4);

            TextBlock txtBlock6 = new TextBlock();
            txtBlock6.Text = "Sistema  ";
            txtBlock6.FontSize = 13;
            txtBlock6.FontWeight = FontWeights.Bold;
            txtBlock6.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock6.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock6.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock6, 0);
            Grid.SetColumn(txtBlock6, 5);

            TextBlock txtBlock7 = new TextBlock();
            txtBlock7.Text = "Registro ";
            txtBlock7.FontSize = 13;
            txtBlock7.FontWeight = FontWeights.Bold;
            txtBlock7.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock7.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock7.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock7, 0);
            Grid.SetColumn(txtBlock7, 6);

            TextBlock txtBlock8 = new TextBlock();
            txtBlock8.Text = "Acción";
            txtBlock8.FontSize = 13;
            txtBlock8.FontWeight = FontWeights.Bold;
            txtBlock8.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock8.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock8.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock8, 0);
            Grid.SetColumn(txtBlock8, 7);

            //// Add column headers to the Grid    
            DynamicGrid.Children.Add(txtBlock1);
            DynamicGrid.Children.Add(txtBlock2);
            DynamicGrid.Children.Add(txtBlock3);
            DynamicGrid.Children.Add(txtBlock4);
            DynamicGrid.Children.Add(txtBlock5);
            DynamicGrid.Children.Add(txtBlock6);
            DynamicGrid.Children.Add(txtBlock7);
            DynamicGrid.Children.Add(txtBlock8);

            // Consulta para el detalle de los cambios
            string strSql = "select * from COT_DATOSEQUIPOS_LOG where aprobado = 0 and SISTEMA = 'APP'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            // Dibujar el detalle
            if (dt.Rows.Count > 0)
            {
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    // Create first Row    
                    TextBlock txtbTransaccion = new TextBlock();
                    txtbTransaccion.Text = dt.Rows[i - 1]["transaccion"].ToString();
                    txtbTransaccion.FontSize = 12;
                    Grid.SetRow(txtbTransaccion, i);
                    Grid.SetColumn(txtbTransaccion, 0);

                    TextBlock txtbNombreEquipo = new TextBlock();
                    txtbNombreEquipo.Text = dt.Rows[i - 1]["nombre_impresora"].ToString();
                    txtbNombreEquipo.FontSize = 11;
                    txtbNombreEquipo.Width = 125;
                    txtbNombreEquipo.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbNombreEquipo, i);
                    Grid.SetColumn(txtbNombreEquipo, 1);

                    TextBlock txtbDireccion = new TextBlock();
                    txtbDireccion.Text = dt.Rows[i - 1]["direccion"].ToString();
                    txtbDireccion.FontSize = 11;
                    txtbDireccion.Width = 125;
                    txtbDireccion.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbDireccion, i);
                    Grid.SetColumn(txtbDireccion, 2);

                    TextBlock txtbAgenciaDepto = new TextBlock();
                    txtbAgenciaDepto.Text = dt.Rows[i - 1]["agencia_departamento"].ToString();
                    txtbAgenciaDepto.FontSize = 11;
                    txtbAgenciaDepto.Width = 125;
                    txtbAgenciaDepto.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbAgenciaDepto, i);
                    Grid.SetColumn(txtbAgenciaDepto, 3);

                    TextBlock txtbFechaRegistro = new TextBlock();
                    txtbFechaRegistro.Text = dt.Rows[i - 1]["fecha_registro"].ToString();
                    txtbFechaRegistro.FontSize = 11;
                    txtbFechaRegistro.Width = 125;
                    txtbFechaRegistro.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbFechaRegistro, i);
                    Grid.SetColumn(txtbFechaRegistro, 4);

                    TextBlock txtbSistema = new TextBlock();
                    txtbSistema.Text = dt.Rows[i - 1]["sistema"].ToString();
                    txtbSistema.FontSize = 12;
                    Grid.SetRow(txtbSistema, i);
                    Grid.SetColumn(txtbSistema, 5);

                    Button btnVer = new Button();
                    btnVer.Content = "VER";
                    btnVer.Click += btnVer_Click;
                    btnVer.Tag = dt.Rows[i - 1]["id"].ToString();
                    Grid.SetRow(btnVer, i);
                    Grid.SetColumn(btnVer, 6);

                    if (dt.Rows[i - 1]["aprobado"] is false)
                    {
                        Button btnAprobado = new Button();
                        btnAprobado.Content = "APROBAR";
                        btnAprobado.Click += btnAprobado_Click;
                        btnAprobado.Tag = dt.Rows[i - 1]["transaccion"].ToString();
                        Grid.SetRow(btnAprobado, i);
                        Grid.SetColumn(btnAprobado, 7);

                        DynamicGrid.Children.Add(btnAprobado);
                    }
                    else
                    {
                        TextBlock txtbAprobado = new TextBlock();
                        txtbAprobado.Text = "Cambio Aprobado ";
                        txtbAprobado.FontSize = 12;
                        Grid.SetRow(txtbAprobado, i);
                        Grid.SetColumn(txtbAprobado, 7);

                        DynamicGrid.Children.Add(txtbAprobado);
                    }

                    DynamicGrid.Children.Add(txtbTransaccion);
                    DynamicGrid.Children.Add(txtbNombreEquipo);
                    DynamicGrid.Children.Add(txtbDireccion);
                    DynamicGrid.Children.Add(txtbAgenciaDepto);
                    DynamicGrid.Children.Add(txtbFechaRegistro);
                    DynamicGrid.Children.Add(txtbSistema);
                    DynamicGrid.Children.Add(btnVer);

                }
            }

            // Display grid into a Window    
            tabPendientes.Content = DynamicGrid;
        }

        // Obtiene los registros de los cambios historicos para los equipos
        private void Obtener_Historico()
        {
            // Create the Grid    
            Grid DynamicGrid = new Grid();
            DynamicGrid.Width = 750;
            DynamicGrid.VerticalAlignment = VerticalAlignment.Top;
            DynamicGrid.Background = new SolidColorBrush(Colors.WhiteSmoke);

            // Create Columns    
            ColumnDefinition gridCol1 = new ColumnDefinition();
            gridCol1.Width = GridLength.Auto;
            ColumnDefinition gridCol2 = new ColumnDefinition();
            ColumnDefinition gridCol3 = new ColumnDefinition();
            ColumnDefinition gridCol4 = new ColumnDefinition();
            ColumnDefinition gridCol5 = new ColumnDefinition();
            gridCol5.Width = GridLength.Auto;
            ColumnDefinition gridCol6 = new ColumnDefinition();
            gridCol6.Width = GridLength.Auto;
            ColumnDefinition gridCol7 = new ColumnDefinition();
            gridCol7.Width = GridLength.Auto;
            DynamicGrid.ColumnDefinitions.Add(gridCol1);
            DynamicGrid.ColumnDefinitions.Add(gridCol2);
            DynamicGrid.ColumnDefinitions.Add(gridCol3);
            DynamicGrid.ColumnDefinitions.Add(gridCol4);
            DynamicGrid.ColumnDefinitions.Add(gridCol5);
            DynamicGrid.ColumnDefinitions.Add(gridCol6);
            DynamicGrid.ColumnDefinitions.Add(gridCol7);

            // Create Rows    
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(25);
            RowDefinition gridRow2 = new RowDefinition();
            gridRow2.Height = new GridLength(25);
            RowDefinition gridRow3 = new RowDefinition();
            gridRow3.Height = new GridLength(25);
            RowDefinition gridRow4 = new RowDefinition();
            gridRow4.Height = new GridLength(25);
            RowDefinition gridRow5 = new RowDefinition();
            gridRow5.Height = new GridLength(25);
            RowDefinition gridRow6 = new RowDefinition();
            gridRow6.Height = new GridLength(25);
            RowDefinition gridRow7 = new RowDefinition();
            gridRow7.Height = new GridLength(25);
            RowDefinition gridRow8 = new RowDefinition();
            gridRow8.Height = new GridLength(25);
            RowDefinition gridRow9 = new RowDefinition();
            gridRow9.Height = new GridLength(25);
            RowDefinition gridRow10 = new RowDefinition();
            gridRow10.Height = new GridLength(25);
            RowDefinition gridRow11 = new RowDefinition();
            gridRow11.Height = new GridLength(25);
            RowDefinition gridRow12 = new RowDefinition();
            gridRow12.Height = new GridLength(25);
            RowDefinition gridRow13 = new RowDefinition();
            gridRow13.Height = new GridLength(25);
            RowDefinition gridRow14 = new RowDefinition();
            gridRow14.Height = new GridLength(25);
            RowDefinition gridRow15 = new RowDefinition();
            gridRow15.Height = new GridLength(25);

            DynamicGrid.RowDefinitions.Add(gridRow1);
            DynamicGrid.RowDefinitions.Add(gridRow2);
            DynamicGrid.RowDefinitions.Add(gridRow3);
            DynamicGrid.RowDefinitions.Add(gridRow4);
            DynamicGrid.RowDefinitions.Add(gridRow5);
            DynamicGrid.RowDefinitions.Add(gridRow6);
            DynamicGrid.RowDefinitions.Add(gridRow7);
            DynamicGrid.RowDefinitions.Add(gridRow8);
            DynamicGrid.RowDefinitions.Add(gridRow9);
            DynamicGrid.RowDefinitions.Add(gridRow10);
            DynamicGrid.RowDefinitions.Add(gridRow11);
            DynamicGrid.RowDefinitions.Add(gridRow12);
            DynamicGrid.RowDefinitions.Add(gridRow13);
            DynamicGrid.RowDefinitions.Add(gridRow14);
            DynamicGrid.RowDefinitions.Add(gridRow15);

            // Add first column header    
            TextBlock txtBlock1 = new TextBlock();
            txtBlock1.Text = " # ";
            txtBlock1.FontSize = 13;
            txtBlock1.FontWeight = FontWeights.Bold;
            txtBlock1.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock1.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock1.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock1, 0);
            Grid.SetColumn(txtBlock1, 0);

            TextBlock txtBlock2 = new TextBlock();
            txtBlock2.Text = "Nombre Impresora";
            txtBlock2.FontSize = 13;
            txtBlock2.FontWeight = FontWeights.Bold;
            txtBlock2.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock2.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock2.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock2, 0);
            Grid.SetColumn(txtBlock2, 1);

            TextBlock txtBlock3 = new TextBlock();
            txtBlock3.Text = "Dirección";
            txtBlock3.FontSize = 13;
            txtBlock3.FontWeight = FontWeights.Bold;
            txtBlock3.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock3.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock3.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock3, 0);
            Grid.SetColumn(txtBlock3, 2);

            TextBlock txtBlock4 = new TextBlock();
            txtBlock4.Text = "Agencia-Depto.";
            txtBlock4.FontSize = 13;
            txtBlock4.FontWeight = FontWeights.Bold;
            txtBlock4.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock4.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock4.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock4, 0);
            Grid.SetColumn(txtBlock4, 3);

            TextBlock txtBlock5 = new TextBlock();
            txtBlock5.Text = "Fecha Registro";
            txtBlock5.FontSize = 13;
            txtBlock5.FontWeight = FontWeights.Bold;
            txtBlock5.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock5.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock5.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock5, 0);
            Grid.SetColumn(txtBlock5, 4);

            TextBlock txtBlock6 = new TextBlock();
            txtBlock6.Text = "Sistema  ";
            txtBlock6.FontSize = 13;
            txtBlock6.FontWeight = FontWeights.Bold;
            txtBlock6.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock6.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock6.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock6, 0);
            Grid.SetColumn(txtBlock6, 5);

            TextBlock txtBlock7 = new TextBlock();
            txtBlock7.Text = "Acción";
            txtBlock7.FontSize = 13;
            txtBlock7.FontWeight = FontWeights.Bold;
            txtBlock7.Foreground = new SolidColorBrush(Colors.WhiteSmoke);
            txtBlock7.Background = new SolidColorBrush(Colors.DarkBlue);
            txtBlock7.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(txtBlock7, 0);
            Grid.SetColumn(txtBlock7, 6);

            //// Add column headers to the Grid    
            DynamicGrid.Children.Add(txtBlock1);
            DynamicGrid.Children.Add(txtBlock2);
            DynamicGrid.Children.Add(txtBlock3);
            DynamicGrid.Children.Add(txtBlock4);
            DynamicGrid.Children.Add(txtBlock5);
            DynamicGrid.Children.Add(txtBlock6);
            DynamicGrid.Children.Add(txtBlock7);

            // Consulta para el detalle de los cambios
            string strSql = "select * from COT_DATOSEQUIPOS_LOG where serie = '" + txtSerie.Text + "'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            // Dibujar el detalle
            if (dt.Rows.Count > 0)
            {
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    // Create first Row    
                    TextBlock txtbTransaccion = new TextBlock();
                    txtbTransaccion.Text = dt.Rows[i - 1]["transaccion"].ToString();
                    txtbTransaccion.FontSize = 12;
                    Grid.SetRow(txtbTransaccion, i);
                    Grid.SetColumn(txtbTransaccion, 0);

                    TextBlock txtbNombreEquipo = new TextBlock();
                    txtbNombreEquipo.Text = dt.Rows[i - 1]["nombre_impresora"].ToString();
                    txtbNombreEquipo.FontSize = 11;
                    txtbNombreEquipo.Width = 125;
                    txtbNombreEquipo.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbNombreEquipo, i);
                    Grid.SetColumn(txtbNombreEquipo, 1);

                    TextBlock txtbDireccion = new TextBlock();
                    txtbDireccion.Text = dt.Rows[i - 1]["direccion"].ToString();
                    txtbDireccion.FontSize = 11;
                    txtbDireccion.Width = 125;
                    txtbDireccion.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbDireccion, i);
                    Grid.SetColumn(txtbDireccion, 2);

                    TextBlock txtbAgenciaDepto = new TextBlock();
                    txtbAgenciaDepto.Text = dt.Rows[i - 1]["agencia_departamento"].ToString();
                    txtbAgenciaDepto.FontSize = 11;
                    txtbAgenciaDepto.Width = 125;
                    txtbAgenciaDepto.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbAgenciaDepto, i);
                    Grid.SetColumn(txtbAgenciaDepto, 3);

                    TextBlock txtbFechaRegistro = new TextBlock();
                    txtbFechaRegistro.Text = dt.Rows[i - 1]["fecha_registro"].ToString();
                    txtbFechaRegistro.FontSize = 11;
                    txtbFechaRegistro.Width = 125;
                    txtbFechaRegistro.HorizontalAlignment = HorizontalAlignment.Left;
                    Grid.SetRow(txtbFechaRegistro, i);
                    Grid.SetColumn(txtbFechaRegistro, 4);

                    // Dibuja el boton de Ver
                    switch (dt.Rows[i - 1]["sistema"].ToString())
                    {

                        case "APP":
                            Button btnVerAPP = new Button();
                            btnVerAPP.Content = "APP";
                            btnVerAPP.Click += btnVer_Click;
                            btnVerAPP.Tag = dt.Rows[i - 1]["id"].ToString();
                            Grid.SetRow(btnVerAPP, i);
                            Grid.SetColumn(btnVerAPP, 5);
                            DynamicGrid.Children.Add(btnVerAPP);
                            break;

                        case "SCON":
                            Button btnVerSCON = new Button();
                            btnVerSCON.Content = "SCON";
                            btnVerSCON.Click += btnVer_Click;
                            btnVerSCON.Tag = dt.Rows[i - 1]["id"].ToString();
                            Grid.SetRow(btnVerSCON, i);
                            Grid.SetColumn(btnVerSCON, 5);
                            DynamicGrid.Children.Add(btnVerSCON);
                            break;

                        case "SISCON":
                            Button btnVerSISCON = new Button();
                            btnVerSISCON.Content = "SISCON";
                            btnVerSISCON.Click += btnVer_Click;
                            btnVerSISCON.Tag = dt.Rows[i - 1]["id"].ToString();
                            Grid.SetRow(btnVerSISCON, i);
                            Grid.SetColumn(btnVerSISCON, 5);
                            DynamicGrid.Children.Add(btnVerSISCON);
                            break;

                        case "EXCEL":
                            Button btnVerEXCEL = new Button();
                            btnVerEXCEL.Content = "EXCEL";
                            btnVerEXCEL.Click += btnVer_Click;
                            btnVerEXCEL.Tag = dt.Rows[i - 1]["id"].ToString();
                            Grid.SetRow(btnVerEXCEL, i);
                            Grid.SetColumn(btnVerEXCEL, 5);
                            DynamicGrid.Children.Add(btnVerEXCEL);
                            break;

                    }

                    // Si el registro es APP dibuja el boton, valida si el cambio fue parobado; de lo contrario dibuja el sistema al cual pertenece
                    if (dt.Rows[i - 1]["sistema"].ToString() == "APP")
                    {
                        if (dt.Rows[i - 1]["aprobado"] is false)
                        {
                            Button btnAprobado = new Button();
                            btnAprobado.Content = "APROBAR";
                            btnAprobado.Click += btnAprobado_Click;
                            btnAprobado.Tag = dt.Rows[i - 1]["transaccion"].ToString();
                            Grid.SetRow(btnAprobado, i);
                            Grid.SetColumn(btnAprobado, 6);

                            DynamicGrid.Children.Add(btnAprobado);
                        }
                        else
                        {
                            TextBlock txtbAprobado = new TextBlock();
                            txtbAprobado.Text = "Cambio Aprobado ";
                            txtbAprobado.FontSize = 12;
                            Grid.SetRow(txtbAprobado, i);
                            Grid.SetColumn(txtbAprobado, 6);

                            DynamicGrid.Children.Add(txtbAprobado);
                        }
                    }
                    else if (dt.Rows[i - 1]["sistema"].ToString() == "EXCEL")
                    {
                        TextBlock txtbAprobado = new TextBlock();
                        txtbAprobado.Text = "Registro Actualizado";
                        txtbAprobado.FontSize = 12;
                        Grid.SetRow(txtbAprobado, i);
                        Grid.SetColumn(txtbAprobado, 6);

                        DynamicGrid.Children.Add(txtbAprobado);
                    }
                    else
                    {
                        TextBlock txtbAprobado = new TextBlock();
                        txtbAprobado.Text = "Registro Anterior";
                        txtbAprobado.FontSize = 12;
                        Grid.SetRow(txtbAprobado, i);
                        Grid.SetColumn(txtbAprobado, 6);

                        DynamicGrid.Children.Add(txtbAprobado);
                    }

                    DynamicGrid.Children.Add(txtbTransaccion);
                    DynamicGrid.Children.Add(txtbNombreEquipo);
                    DynamicGrid.Children.Add(txtbDireccion);
                    DynamicGrid.Children.Add(txtbAgenciaDepto);
                    DynamicGrid.Children.Add(txtbFechaRegistro);
                }
            }
            else
            {
                MessageBox.Show("El número de Serie no tiene datos Históricos!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Display grid into a Window    
            tabHistorico.Content = DynamicGrid;
        }

        // Evento del boton limpiar
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar_Formulario();
        }

        // Evento del boton aprobado
        private void btnAprobado_Click(object sender, RoutedEventArgs e)
        {
            if (Logueado & txtUsuario.Text.ToUpper() == objAutorizaSISCON.Usuario.ToUpper())
            {
                var objResultado = MessageBox.Show("Desea actualizar el registro?", "Datos", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (objResultado == MessageBoxResult.Yes)
                {
                    // Obtiene el boton y sus propiedades
                    Button btnBoton = sender as Button;

                    // Si hay problemas con la actualización; de lo contrario procede a actualizar el estado
                    if (Actualizar_SISCON(Convert.ToInt32(btnBoton.Tag)) == 0) { MessageBox.Show("Hay problemas con actualizar el dato en SISCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }
                    else
                    {
                        MessageBox.Show("Se han actualizado los datos en SISCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information);
                        Actualizar_EstadoBitacora(Convert.ToInt32(btnBoton.Tag));
                    }

                    // Refresca el grid
                    Obtener_Historico();

                    Obtener_Pendientes();

                    Limpiar_Formulario();

                    Aplicar_Permisos();
                }
            }
            else
            {
                MessageBox.Show("Debe de ingresar sus credenciales de SISCON para continuar", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Actualizar el estado del registro en el LOG
        private void Actualizar_EstadoBitacora(int intTransaccion)
        {
            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {

                connUtil.Open();

                SqlCommand command = connUtil.CreateCommand();

                command.Connection = connUtil;

                string strSql = "update COT_DATOSEQUIPOS_LOG set APROBADO = 1, fecha_actualizacion=getdate() where TRANSACCION = " + intTransaccion + " and SISTEMA = 'APP'";

                command.CommandText = strSql;
                int intValida = command.ExecuteNonQuery();
                if (intValida == 0) { MessageBox.Show("Hay problemas con actualizar el estado del registro!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                connUtil.Close();
            }
        }

        // Inserta los datos de los cambios de la APP
        private void Insertar_DatosAnterioresAPP(int intTransaccion)
        {
            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {

                connUtil.Open();

                SqlCommand command = connUtil.CreateCommand();

                command.Connection = connUtil;

                string strSqlAnteriorAPP = "insert into COT_DATOSEQUIPOS_LOG values (" + intTransaccion.ToString() + ",'" + txtNombreImpresora.Text + "','" + txtMacAddress.Text + "','" + txtDireccionIp.Text;
                strSqlAnteriorAPP += "','" + txtIdCliente.Text + "','" + txtNumeroAgencia.Text + "','" + txtCentroComercial.Text;
                strSqlAnteriorAPP += "','" + txtArea.Text + "','" + txtZonaGeografica.Text + "','" + txtAgenciaDepto.Text + "',";
                strSqlAnteriorAPP += "'" + txtEncargado.Text + "','" + txtCuotaFija.Text + "','" + txtTelefonoEncargado.Text;
                strSqlAnteriorAPP += "','" + txtVolumenBn.Text + "','" + txtDireccion.Text + "','" + txtVolumenColor.Text;
                strSqlAnteriorAPP += "',GETDATE(),NULL,'" + txtUsuario.Text + "',0,'" + txtComentarios.Text + "','APP','" + txtSerie.Text + "', '" + txtNivel.Text + "')";

                command.CommandText = strSqlAnteriorAPP;
                int intValida = command.ExecuteNonQuery();
                if (intValida == 0) { MessageBox.Show("Hay problemas con insertar el dato anterior de APP!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                connUtil.Close();
            }
        }

        // Inserta los datos anteriores de SCON
        private void Insertar_DatosAnterioresSCON(int intTransaccion)
        {
            // Obtiene los datos anteriores de SCON
            string strSql = "select Zona, [Agencia/Depto], Contacto, Telefono, [Dirección] from " + SconConexion + "[Datos Actuales] where serie = '" + txtSerie.Text + "'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
                {

                    connUtil.Open();

                    SqlCommand command = connUtil.CreateCommand();

                    command.Connection = connUtil;

                    string strSqlAnteriorSCON = "insert into COT_DATOSEQUIPOS_LOG values (" + intTransaccion.ToString() + ",'','','','','','','','" + dt.Rows[0]["Zona"].ToString() + "','" + dt.Rows[0]["Agencia/Depto"].ToString() + "',";
                    strSqlAnteriorSCON += "'" + dt.Rows[0]["Contacto"].ToString() + "','','" + dt.Rows[0]["Telefono"].ToString() + "','','" + dt.Rows[0]["Dirección"].ToString() + "','',GETDATE(),NULL,'" + txtUsuario.Text + "',0,'','SCON','" + txtSerie.Text + "',NULL)";

                    command.CommandText = strSqlAnteriorSCON;
                    int intValida = command.ExecuteNonQuery();
                    if (intValida == 0) { MessageBox.Show("Hay problemas con insertar el dato anterior de SCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                    connUtil.Close();
                }
            }
        }

        // Inserta los datos anteriores de SISCON
        private void Insertar_DatosAnterioresSISCON(int intTransaccion)
        {
            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {

                connUtil.Open();

                SqlCommand command = connUtil.CreateCommand();

                command.Connection = connUtil;

                string strSqlAnteriorSISCON = "insert into COT_DATOSEQUIPOS_LOG values (" + intTransaccion.ToString() + ",'" + dtDatosAnterior.Rows[0]["nombre_equipo"].ToString() + "','" + dtDatosAnterior.Rows[0]["mac_address"].ToString() + "','" + dtDatosAnterior.Rows[0]["direccion_ip"].ToString();
                strSqlAnteriorSISCON += "','" + dtDatosAnterior.Rows[0]["id_equipo_cliente"].ToString() + "','" + dtDatosAnterior.Rows[0]["numero_agencia"].ToString() + "','" + dtDatosAnterior.Rows[0]["centro_comercial"].ToString();
                strSqlAnteriorSISCON += "','" + dtDatosAnterior.Rows[0]["area"].ToString() + "','" + dtDatosAnterior.Rows[0]["zona_geografica"].ToString() + "','" + dtDatosAnterior.Rows[0]["agencia_depto"].ToString() + "',";
                strSqlAnteriorSISCON += "'" + dtDatosAnterior.Rows[0]["encargado_equipo"].ToString() + "','" + dtDatosAnterior.Rows[0]["cuota_fija"].ToString() + "','" + dtDatosAnterior.Rows[0]["tel_encargado_equipo"].ToString();
                strSqlAnteriorSISCON += "','" + dtDatosAnterior.Rows[0]["volumen_bn"].ToString() + "','" + dtDatosAnterior.Rows[0]["direccion_equi_empresa"].ToString() + "','" + dtDatosAnterior.Rows[0]["volumen_color"].ToString();
                strSqlAnteriorSISCON += "',GETDATE(),NULL,'" + txtUsuario.Text + "',0,'" + txtComentarios.Text + "','SISCON','" + txtSerie.Text + "','" + dtDatosAnterior.Rows[0]["nivel"].ToString() + "')";

                command.CommandText = strSqlAnteriorSISCON;
                int intValida = command.ExecuteNonQuery();
                if (intValida == 0) { MessageBox.Show("Hay problemas con insertar el dato anterior de SISCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                connUtil.Close();
            }
        }

        // Completa la direccion en el textbox para guardar en SISCON
        private void Completar_Direccion()
        {
            // Completa la dirección
            if (txtNivelGeografico.Text == "") { txtDireccion2.Text = txtDireccion1.Text + " Zona " + txtZona.Text + cmbCiudad.SelectedItem.ToString().Substring(37, (cmbCiudad.SelectedItem.ToString().Length - 37)) + " " + cmbDepartamento.SelectedItem.ToString(); }
            else { txtDireccion2.Text = txtDireccion1.Text + " Zona " + txtZona.Text + " Nivel " + txtNivelGeografico.Text + cmbCiudad.SelectedItem.ToString().Substring(37, (cmbCiudad.SelectedItem.ToString().Length - 37)) + " " + cmbDepartamento.SelectedItem.ToString(); }

            MessageBox.Show("Dirección completa!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information);

            txtDireccion.Text = txtDireccion2.Text;
            txtZonaGeografica.Text = txtZona.Text;
            txtNivel.Text = txtNivelGeografico.Text;

            tabDatos.Focus();
        }

        // Evento del boton completar direccion
        private void btnObtenerDireccion_Click(object sender, RoutedEventArgs e)
        {
            tabDireccion.IsEnabled = true;
            tabDireccion.Focus();
        }

        // Evento del boton completar direccion
        private void btnCompletar_Click(object sender, RoutedEventArgs e)
        {
            Completar_Direccion();
        }

        // Evento buscar del boton 
        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            Aplicar_Permisos();

            if (Consultar_SeriesSISCON()) { Consultar_SerieSCON(); }
            else 
            { 
                MessageBox.Show("La serie esta desactiva en SISCON, no es posible actualizarla!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                Limpiar_Formulario();
            }
        }

        // Evento del boton login
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Si el boton esta en modo Login
            if (btnLogin.Content.ToString() == "Login")
            {
                if (Validar_Credenciales())
                {
                    // Deshabilita los controles y login
                    txtUsuario.IsEnabled = false;
                    txtClave.IsEnabled = false;
                    Logueado = true;
                    btnLogin.Content = "Logout";

                    // Verifica si esta en modo consulta
                    if (lblSistema.Visibility == Visibility.Visible) { Limpiar_Formulario(); }
                    else { Aplicar_Permisos(); }

                    // Dirige el foto al formulario de datos
                    tabDatos.Focus();
                }
                else { MessageBox.Show("Debe de ingresar sus credenciales de SISCON para continuar!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
            else
            {
                // Habilita los controles y logout
                txtUsuario.IsEnabled = true;
                txtUsuario.Text = "";
                txtClave.IsEnabled = true;
                txtClave.Password = "";
                Logueado = false;
                btnLogin.Content = "Login";
                Departamento = 0;
                lblnombreusuario.Visibility = Visibility.Hidden;
                lblareausuario.Visibility = Visibility.Hidden;

                Limpiar_Formulario();

                // Dirige el foto al formulario de datos
                tabDatos.Focus();
            }
        }

        // Aplica los permisos por departamento
        private void Aplicar_Permisos()
        {
            // Si el departamento es soluciones
            switch (Departamento)
            {
                case 2:
                    txtCuotaFija.IsEnabled = true;
                    txtVolumenBn.IsEnabled = true;
                    txtVolumenColor.IsEnabled = true;
                    txtComentarios.IsEnabled = true;
                    tabPendientes.IsEnabled = true;
                    break;
                case 13:
                    txtNombreImpresora.IsEnabled = true;
                    txtMacAddress.IsEnabled = true;
                    txtDireccionIp.IsEnabled = true;
                    txtIdCliente.IsEnabled = true;
                    txtNumeroAgencia.IsEnabled = true;
                    txtCentroComercial.IsEnabled = true;
                    txtArea.IsEnabled = true;
                    txtZonaGeografica.IsEnabled = true;
                    txtNivel.IsEnabled = true;
                    txtAgenciaDepto.IsEnabled = true;
                    txtEncargado.IsEnabled = true;
                    txtTelefonoEncargado.IsEnabled = true;
                    txtComentarios.IsEnabled = true;
                    //txtDireccion.IsEnabled = true;
                    btnObtenerDireccion.IsEnabled = true;
                    break;
            }
        }

        // Evento actualizar del boton
        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            // Si esta logueado le permite continuar
            if (Logueado)
            {
                var objResultado = MessageBox.Show("Desea actualizar el registro?", "Datos", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (objResultado == MessageBoxResult.Yes)
                {
                    // Obtiene el Id de transacción
                    intIdTransaccion = Obtener_IdTransaccion();

                    // Inserta los registros anteriores en las tablas
                    Insertar_DatosAnterioresSISCON(intIdTransaccion);

                    Insertar_DatosAnterioresSCON(intIdTransaccion);

                    Insertar_DatosAnterioresAPP(intIdTransaccion);

                    // Verifica el departamento para actualizar en SISCON
                    if (Departamento == 13)
                    {
                        Actualizar_SCON();
                    }
                    else { MessageBox.Show("No tiene permisos para actualizar datos en SCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                    MessageBox.Show("Se ha generado la transacción " + intIdTransaccion.ToString() + ", pendiente de autorizar para actualizar en SISCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresca el grid
                    Obtener_Historico();

                    Obtener_Pendientes();

                    // Envia el correo de alerta
                    Enviar_CorreoInterno(objAutorizaSISCON.CorreoElectronico, "Solicitud de Cambio en Serie", Generar_CuerpoCorreo());

                    Limpiar_Formulario();
                }
            }
            else
            {
                MessageBox.Show("Debe de ingresar sus credenciales de SISCON para continuar", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Actualizar el registro en SISCON
        private int Actualizar_SISCON(int intTransaccion)
        {
            // Obtiene los datos pendientes de autorizar / actualizar para SISCON
            string strSql = "select * from COT_DATOSEQUIPOS_LOG where TRANSACCION = " + intTransaccion.ToString() + " and SISTEMA = 'APP'";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {

                // Valida campos numericos
                float fltCuota = 0;
                float fltVolumenBN = 0;
                float fltVolumenColor = 0;

                if (dt.Rows[0]["cuota_fija"].ToString() != "") { fltCuota = float.Parse(dt.Rows[0]["cuota_fija"].ToString()); }
                if (dt.Rows[0]["volumen_bn"].ToString() != "") { fltVolumenBN = float.Parse(dt.Rows[0]["volumen_bn"].ToString()); }
                if (dt.Rows[0]["volumen_color"].ToString() != "") { fltVolumenColor = float.Parse(dt.Rows[0]["volumen_color"].ToString()); }

                using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
                {

                    connUtil.Open();

                    SqlCommand command = connUtil.CreateCommand();

                    command.Connection = connUtil;

                    string strSqlSISCON = "update COT_CONTRATOS_DET_DESPACHO set NOMBRE_EQUIPO = '" + dt.Rows[0]["nombre_impresora"].ToString() + "', MAC_ADDRESS = '" + dt.Rows[0]["mac_address"].ToString() + "', DIRECCION_IP='" + dt.Rows[0]["direccion_ip"].ToString() + "',";
                    strSqlSISCON += "ID_EQUIPO_CLIENTE='" + dt.Rows[0]["id_cliente"].ToString() + "', NUMERO_AGENCIA='" + dt.Rows[0]["numero_agencia"].ToString() + "',CENTRO_COMERCIAL='" + dt.Rows[0]["centro_comercial"].ToString() + "',";
                    strSqlSISCON += "AREA='" + dt.Rows[0]["area"].ToString() + "', ZONA_GEOGRAFICA='" + dt.Rows[0]["zona_geografica"].ToString() + "', AGENCIA_DEPTO='" + dt.Rows[0]["agencia_departamento"].ToString() + "',";
                    strSqlSISCON += "ENCARGADO_EQUIPO='" + dt.Rows[0]["encargado"].ToString() + "', CUOTA_FIJA=" + fltCuota.ToString() + ", TEL_ENCARGADO_EQUIPO='" + dt.Rows[0]["telefono_encargado"].ToString() + "',";
                    strSqlSISCON += "VOLUMEN_BN=" + fltVolumenBN.ToString() + ", DIRECCION_EQUI_EMPRESA='" + dt.Rows[0]["direccion"].ToString() + "',VOLUMEN_COLOR=" + fltVolumenColor.ToString() + ",NIVEL='" + dt.Rows[0]["nivel"].ToString() + "'";
                    strSqlSISCON += " where NUMERO_SERIE = '" + txtSerie.Text + "' and ESTATUS_ARTICULO = 1";

                    command.CommandText = strSqlSISCON;
                    int intResultado = command.ExecuteNonQuery();

                    connUtil.Close();

                    return intResultado;
                }
            }
            else { return 0; }
        }

        // Actualizar el registro en SCON
        private void Actualizar_SCON()
        {
            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                connUtil.Open();

                SqlCommand command = connUtil.CreateCommand();

                command.Connection = connUtil;

                string strSqlSCON = "update " + SconConexion + "[Datos Actuales] set Zona = '" + txtZonaGeografica.Text + "', [Agencia/Depto] = '" + txtAgenciaDepto.Text + "', Contacto = '" + txtEncargado.Text + "', Telefono = '" + txtTelefonoEncargado.Text + "',";
                strSqlSCON += "[Dirección] = '" + txtDireccion.Text + "', DeptoId = " + cmbDepartamento.SelectedIndex + ", CiudadId=" + cmbCiudad.SelectedIndex + ", Comentario='" + txtComentarios.Text + "   IP: " + txtDireccionIp.Text + "' ";
                strSqlSCON += "where serie = '" + txtSerie.Text + "'";

                command.CommandText = strSqlSCON;
                int intValida = command.ExecuteNonQuery();
                if (intValida == 0) { MessageBox.Show("Hay problemas con actualizar el dato en SCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }
                else { MessageBox.Show("Se han actualizado los datos en SCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Information); }

                connUtil.Close();
            }
        }

        // Evento actualizar del boton
        private void btnVer_Click(object sender, RoutedEventArgs e)
        {
            // Obtiene el boton y sus propiedades
            Button btnBoton = sender as Button;

            // Rellena el formulario con los datos solicitados
            Consultar_Bitacora(Convert.ToInt32(btnBoton.Tag));
        }

        // Rellena el formulario con el dato solicitado
        private void Consultar_Bitacora(int intID)
        {
            string strSql = @"select * from COT_DATOSEQUIPOS_LOG where id = " + intID.ToString();

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                // Rellena el formulario
                txtNombreImpresora.Text = dt.Rows[0]["nombre_impresora"].ToString();
                txtNombreImpresora.IsEnabled = false;
                txtMacAddress.Text = dt.Rows[0]["mac_address"].ToString();
                txtMacAddress.IsEnabled = false;
                txtDireccionIp.Text = dt.Rows[0]["direccion_ip"].ToString();
                txtDireccionIp.IsEnabled = false;
                txtIdCliente.Text = dt.Rows[0]["id_cliente"].ToString();
                txtIdCliente.IsEnabled = false;
                txtNumeroAgencia.Text = dt.Rows[0]["numero_agencia"].ToString();
                txtNumeroAgencia.IsEnabled = false;
                txtCentroComercial.Text = dt.Rows[0]["centro_comercial"].ToString();
                txtCentroComercial.IsEnabled = false;
                txtArea.Text = dt.Rows[0]["area"].ToString();
                txtArea.IsEnabled = false;
                txtZonaGeografica.Text = dt.Rows[0]["zona_geografica"].ToString();
                txtZonaGeografica.IsEnabled = false;
                txtAgenciaDepto.Text = dt.Rows[0]["agencia_departamento"].ToString();
                txtAgenciaDepto.IsEnabled = false;
                txtEncargado.Text = dt.Rows[0]["encargado"].ToString();
                txtEncargado.IsEnabled = false;
                txtTelefonoEncargado.Text = dt.Rows[0]["telefono_encargado"].ToString();
                txtTelefonoEncargado.IsEnabled = false;
                txtCuotaFija.Text = dt.Rows[0]["cuota_fija"].ToString();
                txtCuotaFija.IsEnabled = false;
                txtVolumenBn.Text = dt.Rows[0]["volumen_bn"].ToString();
                txtVolumenBn.IsEnabled = false;
                txtVolumenColor.Text = dt.Rows[0]["volumen_color"].ToString();
                txtVolumenColor.IsEnabled = false;
                txtDireccion.Text = dt.Rows[0]["direccion"].ToString();
                txtDireccion.IsEnabled = false;
                txtDireccion1.Text = txtDireccion.Text;
                txtComentarios.Text = dt.Rows[0]["comentarios"].ToString();
                txtComentarios.IsEnabled = false;
                txtNivel.Text = dt.Rows[0]["nivel"].ToString();
                txtNivel.IsEnabled = false;
                txtSerie.Text = dt.Rows[0]["serie"].ToString();

                // Agrega los datos de la bicatora
                lblSistema.Content = "Sistema: " + dt.Rows[0]["sistema"].ToString();
                lblSistema.Visibility = Visibility.Visible;
                lblUsuario.Content = "Usuario: " + dt.Rows[0]["usuario"].ToString();
                lblUsuario.Visibility = Visibility.Visible;
                lblFechaRegistro.Content = "Fecha Registro: " + dt.Rows[0]["fecha_registro"].ToString();
                lblFechaRegistro.Visibility = Visibility.Visible;
                lblFechaActual.Content = "Fecha Actualizado: " + dt.Rows[0]["fecha_actualizacion"].ToString();
                lblFechaActual.Visibility = Visibility.Visible;
                lblTransaccion.Content = "Transacción: " + dt.Rows[0]["transaccion"].ToString();
                lblTransaccion.Visibility = Visibility.Visible;
                if (dt.Rows[0]["sistema"].ToString() == "APP")
                {
                    if (Convert.ToBoolean(dt.Rows[0]["aprobado"]))
                    {
                        lblAprobado.Content = "Cambio Aprobado";
                        lblAprobado.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        lblAprobado.Content = "Cambio No Aprobado";
                        lblAprobado.Visibility = Visibility.Visible;
                    }
                }

                // Coloca el foco en el formulario de datos
                tabDatos.Focus();
                btnCancelar.IsEnabled = true;

                // Habilita los comentarios del registro
                lblComentarios.Content = "Este detalle fue obtenido desde la bitácora!";
                lblComentarios.Visibility = Visibility.Visible;

            }
            else { MessageBox.Show("Se tienen problemas para obtener el dato de la bitácora!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

        }

        // Evento del combo departamento
        private void cmbDepartamento_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Llenar_ComboCiudad(cmbDepartamento.SelectedIndex);
        }

        // Llena el combo de ciudades desde SCON
        private void Llenar_ComboCiudad(int intCiudad)
        {
            string strSql = @"select * from " + SconConexion + "[Ciudades] where deptoid = " + intCiudad.ToString();

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                // Limpia el combo
                cmbCiudad.Items.Clear();

                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    ComboBoxItem cmbItem = new ComboBoxItem();
                    cmbItem.Content = dt.Rows[i - 1]["Nombre de Ciudad"].ToString();
                    cmbItem.Tag = Convert.ToInt16(dt.Rows[i - 1]["ciudadid"].ToString());

                    // Asigna los datos al combo
                    cmbCiudad.Items.Add(cmbItem);
                }
            }
        }

        // Llena el combo de departamentos desde SCON
        private void Llenar_ComboDepartamento()
        {
            string strSql = @"select * from " + SconConexion + "[Depto]";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 0)
            {
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    cmbDepartamento.Items.Insert(Convert.ToInt32(dt.Rows[i - 1]["deptoid"].ToString()), dt.Rows[i - 1]["departamento"].ToString());
                }
            }
        }

        // Metodo que valida si la serie tiene problemas en SISCON
        private bool Validar_ProblemasSISCON()
        {
            string strSql = @"select * from cot_contratos_det_despacho where numero_serie = '" + txtSerie.Text + "' and estatus_articulo = 1";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            if (dt.Rows.Count > 1)
            {
                return true;
            }
            else { return false; }
        }

        // Obtiene la serie de SISCON
        private void Consultar_SerieSCON()
        {
            // Valida que la serie tenga datos
            if (txtSerie.Text != "")
            {
                string strSql = @"select Serie, Cliente, [Agencia/Depto], [Dirección], Zona, Telefono, Comentario, DeptoId, CiudadId from" + SconConexion + "[Datos Actuales] where serie = '";
                strSql += txtSerie.Text + "'";

                DataTable dt = new DataTable();

                using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                    {
                        connUtil.Open();

                        da.Fill(dt);

                        connUtil.Close();
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    // Coloca datos de cliente y de contrato
                    lblContrato.Visibility = Visibility.Visible;
                    lblContrato.Content = "";
                    lblCliente.Visibility = Visibility.Visible;
                    lblCliente.Content = dt.Rows[0]["cliente"].ToString();

                    // Rellena el formulario
                    txtZonaGeografica.Text = dt.Rows[0]["zona"].ToString();
                    txtAgenciaDepto.Text = dt.Rows[0]["Agencia/Depto"].ToString();
                    txtDireccion.Text = dt.Rows[0]["Dirección"].ToString();
                    txtDireccion1.Text = txtDireccion.Text;
                    txtComentarios.Text = dt.Rows[0]["Comentario"].ToString();

                    // Habilita los objetos
                    btnActualizar.IsEnabled = true;
                    btnCancelar.IsEnabled = true;
                    tabHistorico.IsEnabled = true;

                    // Traslada los datos anteriores de SISCON al objetvo
                    dtDatosAnterior = Consultar_SeriesSISCON(txtSerie.Text);

                    // Llena el Tab de datos Historicos
                    Obtener_Historico();

                    // Colocar una nota del registro
                    lblComentarios.Content = "Registro actual de SCON!";
                    lblComentarios.Visibility = Visibility.Visible;

                    // Combos de direccion
                    Llenar_ComboDepartamento();
                }
                else
                {
                    MessageBox.Show("El número de Serie no esta registrado en SCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);

                    Limpiar_Formulario();
                }

            }
            else
            {
                MessageBox.Show("El número de Serie no puede estar en blanco!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);

                Limpiar_Formulario();
            }
        }

        // Consulta las series de SISCON
        private DataTable Consultar_SeriesSISCON(string strSerie)
        {
            string strSql = @"select DET.CONTRATO_NO, ENC.NOMBRE_CLIENTE, DET.NOMBRE_EQUIPO, DET.MAC_ADDRESS, DET.DIRECCION_IP, DET.ID_EQUIPO_CLIENTE, DET.NUMERO_AGENCIA, DET.CENTRO_COMERCIAL, DET.AREA, DET.ZONA_GEOGRAFICA, 
                                DET.AGENCIA_DEPTO, DET.ENCARGADO_EQUIPO, DET.TEL_ENCARGADO_EQUIPO, DET.CUOTA_FIJA, DET.VOLUMEN_BN, DET.VOLUMEN_COLOR, DET.DIRECCION_EQUI_EMPRESA, DET.NIVEL 
                                from COT_CONTRATOS_DET_DESPACHO DET, COT_CONTRATOS_ENC ENC
                                where ENC.CONTRATO_NO = DET.CONTRATO_NO
                                and DET.NUMERO_SERIE = '";
            strSql += strSerie + "' and DET.ESTATUS_ARTICULO = 1";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            return dt; 
        }

        // Valida si la serie en SISCON esta activa
        private bool Consultar_SeriesSISCON()
        {
            string strSql = @"select DET.CONTRATO_NO, ENC.NOMBRE_CLIENTE, DET.NOMBRE_EQUIPO, DET.MAC_ADDRESS, DET.DIRECCION_IP, DET.ID_EQUIPO_CLIENTE, DET.NUMERO_AGENCIA, DET.CENTRO_COMERCIAL, DET.AREA, DET.ZONA_GEOGRAFICA, 
                                DET.AGENCIA_DEPTO, DET.ENCARGADO_EQUIPO, DET.TEL_ENCARGADO_EQUIPO, DET.CUOTA_FIJA, DET.VOLUMEN_BN, DET.VOLUMEN_COLOR, DET.DIRECCION_EQUI_EMPRESA, DET.NIVEL 
                                from COT_CONTRATOS_DET_DESPACHO DET, COT_CONTRATOS_ENC ENC
                                where ENC.CONTRATO_NO = DET.CONTRATO_NO
                                and DET.NUMERO_SERIE = '";
            strSql += txtSerie.Text + "' and DET.ESTATUS_ARTICULO = 1";

            DataTable dt = new DataTable();

            using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                {
                    connUtil.Open();

                    da.Fill(dt);

                    connUtil.Close();
                }
            }

            // Si la serie esta desactiva returna false
            if (dt.Rows.Count == 0) { return false; }
            else { return true; }
        }

        // Obtiene la serie de SISCON
        private void Consultar_SerieSISCON()
        {
            // Valida que la serie tenga datos
            if (txtSerie.Text != "")
            {
                // Validamos si la serie tiene problemas en SISCON
                if (Validar_ProblemasSISCON()) { MessageBox.Show("El número de Serie tiene problemas en SISCON, no se actualizarán los datos!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error); }

                string strSql = @"select DET.CONTRATO_NO, ENC.NOMBRE_CLIENTE, DET.NOMBRE_EQUIPO, DET.MAC_ADDRESS, DET.DIRECCION_IP, DET.ID_EQUIPO_CLIENTE, DET.NUMERO_AGENCIA, DET.CENTRO_COMERCIAL, DET.AREA, DET.ZONA_GEOGRAFICA, 
                                DET.AGENCIA_DEPTO, DET.ENCARGADO_EQUIPO, DET.TEL_ENCARGADO_EQUIPO, DET.CUOTA_FIJA, DET.VOLUMEN_BN, DET.VOLUMEN_COLOR, DET.DIRECCION_EQUI_EMPRESA, DET.NIVEL 
                                from COT_CONTRATOS_DET_DESPACHO DET, COT_CONTRATOS_ENC ENC
                                where ENC.CONTRATO_NO = DET.CONTRATO_NO
                                and DET.NUMERO_SERIE = '";
                strSql += txtSerie.Text + "' and DET.ESTATUS_ARTICULO = 1";

                DataTable dt = new DataTable();

                using (SqlConnection connUtil = new SqlConnection(connStringSISCON))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(strSql, connUtil))
                    {
                        connUtil.Open();

                        da.Fill(dt);

                        connUtil.Close();
                    }
                }

                if (dt.Rows.Count > 0)
                {
                    // Coloca datos de cliente y de contrato
                    lblContrato.Visibility = Visibility.Visible;
                    lblContrato.Content = dt.Rows[0]["contrato_no"].ToString();
                    lblCliente.Visibility = Visibility.Visible;
                    lblCliente.Content = dt.Rows[0]["nombre_cliente"].ToString();

                    // Rellena el formulario
                    txtNombreImpresora.Text = dt.Rows[0]["nombre_equipo"].ToString();
                    txtMacAddress.Text = dt.Rows[0]["mac_address"].ToString();
                    txtDireccionIp.Text = dt.Rows[0]["direccion_ip"].ToString();
                    txtIdCliente.Text = dt.Rows[0]["id_equipo_cliente"].ToString();
                    txtNumeroAgencia.Text = dt.Rows[0]["numero_agencia"].ToString();
                    txtCentroComercial.Text = dt.Rows[0]["centro_comercial"].ToString();
                    txtArea.Text = dt.Rows[0]["area"].ToString();
                    txtZonaGeografica.Text = dt.Rows[0]["zona_geografica"].ToString();
                    txtAgenciaDepto.Text = dt.Rows[0]["agencia_depto"].ToString();
                    txtEncargado.Text = dt.Rows[0]["encargado_equipo"].ToString();
                    txtTelefonoEncargado.Text = dt.Rows[0]["tel_encargado_equipo"].ToString();
                    txtCuotaFija.Text = dt.Rows[0]["cuota_fija"].ToString();
                    txtVolumenBn.Text = dt.Rows[0]["volumen_bn"].ToString();
                    txtVolumenColor.Text = dt.Rows[0]["volumen_color"].ToString();
                    txtDireccion.Text = dt.Rows[0]["direccion_equi_empresa"].ToString();
                    txtNivel.Text = dt.Rows[0]["nivel"].ToString();
                    txtDireccion1.Text = txtDireccion.Text;

                    // Habilita los objetos
                    btnActualizar.IsEnabled = true;
                    btnCancelar.IsEnabled = true;
                    tabHistorico.IsEnabled = true;

                    // Traslada los datos anteriores de SISCON al objetvo
                    dtDatosAnterior = dt;

                    // Llena el Tab de datos Historicos
                    Obtener_Historico();

                    // Colocar una nota del registro
                    lblComentarios.Content = "Registro actual de SISCON!";
                    lblComentarios.Visibility = Visibility.Visible;

                    // Combos de direccion
                    Llenar_ComboDepartamento();
                }
                else
                {
                    MessageBox.Show("El número de Serie no esta registrado en SISCON!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);

                    Limpiar_Formulario();
                }

            }
            else
            {
                MessageBox.Show("El número de Serie no puede estar en blanco!", "Datos", MessageBoxButton.OK, MessageBoxImage.Error);

                Limpiar_Formulario();
            }
        }
    }
}
