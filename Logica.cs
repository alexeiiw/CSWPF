using System;
using Banca.Utilidades;
using ADOR;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Canella;
using System.Data;
using Banca.Clonacion.Configuraciones;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Banca.Servicios.ACHConsultaCuenta.App_Code
{
    public class Logica
    {
        // Variables a nivel de clase
        string strRespuestaServicio = "";
        string strJsonSolicitud;
        string strJsonRespuesta;
        string strEstadoToken;
        string strNombreCuenta;
        string strSolicitudToken;
        string strRespuestaToken;

        // Metodo que valida la cuenta BI y otorga respuesta a ICG
        public Respuesta ConsultarCuentaBI (Solicitud objSolicitud, string strToken)
        {
            // Define variables a nivel de clase
            bool blnEncontrado = true;

            // Obtiene el tipo para la respuesta
            var objTypeRespuesta = Configuraciones.ObtenerValor(Constantes.Configuracion.TypeRespuesta);

            // Valida la integridad de la solicitud
            var objJsonSolicitud = JsonConvert.SerializeObject(objSolicitud);

            try
            {
                if (objSolicitud.Destination == null || objSolicitud.Destination == "" ||
                    objSolicitud.type == null || objSolicitud.type == "" ||
                    objSolicitud.payload.account == null || objSolicitud.payload.account == "" ||
                    objSolicitud.payload.currency == 0 || objSolicitud.payload.product == 0 ||
                    objSolicitud.Source == null || objSolicitud.Source == "" ||
                    objSolicitud.UUID == null || objSolicitud.UUID == "")
                {
                    Respuesta objRespuesta = GenerarMensajeRespuesta(objSolicitud.Source, objTypeRespuesta.ToString(), "false", null,
                                                                    objSolicitud.Destination, objSolicitud.UUID);

                    // Serializa el Json de respuesta
                    var objJsonRespuesta = JsonConvert.SerializeObject(objRespuesta);

                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(0, 0, null, objSolicitud.Destination, objSolicitud.Source,
                        objJsonSolicitud.ToString(), objJsonRespuesta.ToString(), 1,
                        App_Code.Constantes.Mensaje.Mensaje11ErrorJson);

                    return objRespuesta;
                }
            }
            catch (Exception e)
            {
                Respuesta objRespuesta = GenerarMensajeRespuesta(objSolicitud.Source, objTypeRespuesta.ToString(), "false", null,
                                                objSolicitud.Destination, objSolicitud.UUID);

                // Serializa el Json de respuesta
                var objJsonRespuesta = JsonConvert.SerializeObject(objRespuesta);

                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(0, 0, null, objSolicitud.Destination, objSolicitud.Source,
                    objJsonSolicitud.ToString(), objJsonRespuesta.ToString(), 1,
                    App_Code.Constantes.Mensaje.Mensaje11ErrorJson + " " + e.ToString());

                return objRespuesta;
            }

            //// Valida que el tipo de moneda sea 1 o 2 
            //if (objSolicitud.payload.currency != 1 & objSolicitud.payload.currency !=2)
            //{
            //    Respuesta objRespuesta = GenerarMensajeRespuesta(objSolicitud.Source, objTypeRespuesta[0].Value.ToString(), "false", null,
            //        objSolicitud.Destination, objSolicitud.UUID);

            //    // Serializa el Json de respuesta
            //    var objJsonRespuesta = JsonConvert.SerializeObject(objRespuesta);

            //    // Registra evento en Bicatora
            //    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
            //    objBitacora.RegistrarBitacora(0, 0, null, objSolicitud.Destination, objSolicitud.Source,
            //        objJsonSolicitud.ToString(), objJsonRespuesta.ToString(), 1, 
            //        App_Code.Constantes.Mensaje.Mensaje10ErrorTipoMoneda);

            //    return objRespuesta;
            //}

            // Variable para la cuenta
            string strCuenta = "----------";

            // Si el producto es 1 o 2
            if (objSolicitud.payload.product == 1 || objSolicitud.payload.product == 2)
            {
                // Convertir la cuenta a formato BI ya que viene en IBAN
                BibliotecaCFND2BLD.CLCFND2BLD objCuentaIBAN = new BibliotecaCFND2BLD.CLCFND2BLD();
                string strCuentaIBAN = objSolicitud.payload.account.Trim();
                string strTipoCuenta = objSolicitud.payload.product.ToString();
                short codRet = 99;

                //strCuenta = objSolicitud.payload.account.ToString().Trim(); // Bypass
                try
                {
                    objCuentaIBAN.CFND2BLD(ref strCuentaIBAN, ref strTipoCuenta, ref strCuenta, ref codRet);

                    if (codRet != 0) // Si tiene problemas al convertir la cuenta IBAN a regular
                    {
                        Respuesta objRespuesta = GenerarMensajeRespuesta(objSolicitud.Source, objTypeRespuesta.ToString(), "false", null,
                            objSolicitud.Destination, objSolicitud.UUID);

                        // Serializa el Json de respuesta
                        var objJsonRespuesta = JsonConvert.SerializeObject(objRespuesta);

                        // Registra evento en Bicatora
                        App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                        objBitacora.RegistrarBitacora(0, 0, null, objSolicitud.Destination, objSolicitud.Source,
                            objJsonSolicitud.ToString(), objJsonRespuesta.ToString(), 1,
                            App_Code.Constantes.Mensaje.Mensaje22ErrorCuentaIBAN);

                        return objRespuesta;
                    }
                }
                catch (Exception e)
                {
                    // Registra en Bitacora de Aplicacion
                    BitacoraAplicacion.RegistrarError(e.ToString());

                    Respuesta objRespuesta = GenerarMensajeRespuesta(objSolicitud.Source, objTypeRespuesta.ToString(), "false", null,
                                    objSolicitud.Destination, objSolicitud.UUID);

                    // Serializa el Json de respuesta
                    var objJsonRespuesta = JsonConvert.SerializeObject(objRespuesta);

                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(0, 0, null, objSolicitud.Destination, objSolicitud.Source,
                        objJsonSolicitud.ToString(), objJsonRespuesta.ToString(), 1,
                        App_Code.Constantes.Mensaje.Mensaje1Excepcion + " " + e.ToString());

                    return objRespuesta;
                }
            }
            else // Valida la longitud de la cuenta
            {
                if (objSolicitud.payload.product == 4)
                {
                    if (objSolicitud.payload.account.Length > 16) { strCuenta = objSolicitud.payload.account.Trim().Substring(0, 16); }
                }
                if (objSolicitud.payload.product == 3)
                {
                    if (objSolicitud.payload.account.Length > 14) { strCuenta = objSolicitud.payload.account.Trim().Substring(0, 14); }
                }
            }

            // Define los mensajes que va a recibir por referencia
            string strDescripcion = "";

            // Validacion de cuentas - version 2 VB
            int intEstado = verificaSituacionCuentaXMoneda(strCuenta, objSolicitud.payload.currency, objSolicitud.payload.product,
                                                            ref blnEncontrado, ref strDescripcion);
            if (!blnEncontrado)
            {
                Respuesta objRespuesta = GenerarMensajeRespuesta(objSolicitud.Source, objTypeRespuesta.ToString(), "false", null,
                                                                    objSolicitud.Destination, objSolicitud.UUID);

                // Serializa el Json de respuesta
                var objJsonRespuesta = JsonConvert.SerializeObject(objRespuesta);

                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(0, 0, null, objSolicitud.Destination, objSolicitud.Source,
                    objJsonSolicitud.ToString(), objJsonRespuesta.ToString(), 1,
                    strDescripcion);

                return objRespuesta;
            }

            if (blnEncontrado)  // Cuenta encontrada
            {
                Respuesta objRespuesta = GenerarMensajeRespuesta(objSolicitud.Source, objTypeRespuesta.ToString(), "true", DarFormatoNombre(strNombreCuenta),
                                                objSolicitud.Destination, objSolicitud.UUID);

                // Serializa el Json de respuesta
                var objJsonRespuesta = JsonConvert.SerializeObject(objRespuesta);

                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(0, 0, null, objSolicitud.Destination, objSolicitud.Source,
                    objJsonSolicitud.ToString(), objJsonRespuesta.ToString(), 0,
                    App_Code.Constantes.Mensaje.Mensaje0Ok);

                return objRespuesta;
            }

            return null;
        }

        // Realiza la validacion y consume el servicio de ICG para validar si la cuenta existe en el banco destino
        public ValidaCuenta ConsultarCuentaOB(int intInstalacion, string strUsuario, byte bytCanal, string cuenta, int banco, byte moneda, byte tipocuenta)
        {
            // Devine variables a nivel de metodo
            string strCuenta = "";
            byte bytMoneda = 0;
            int lngBanco = 0;
            byte bytTipo = 0;
            string StrCuentaIBAN = "";
            string strToken = "";
            bool blnExisteToken = ValidarToken(3, ref strToken); // Valida que el token guardado sea valido

            /* 
            // Valida que le banco destino tenga la consulta en linea habilitada
            Conexion objConexion1 = new Conexion();

            // Recibe los parametros, crea contenedor y parametro
            SpParameters lstParametros1 = new SpParameters();
            SpParameter objParametro1 = new SpParameter();

            objParametro1 = new SpParameter();
            objParametro1.Nombre = "codbanco";
            objParametro1.Tipo = objParametro1.entero;
            objParametro1.Valor = banco.ToString();
            lstParametros1.Add(objParametro1);

            DataSet dsResultado1 = objConexion1.conexion_ConsultaReturnDataSet(App_Code.Constantes.Configuracion.SPPermiteConsulta, lstParametros1);

            // Si el banco destino no acepta la consulta en linea devuelve error
            if (dsResultado1 == null)
            {
                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                    App_Code.Constantes.Mensaje.Mensaje7ErrorPermiteConsulta, 1);

                // Retorna la excepcion
                ValidaCuenta objValidaCuenta = new ValidaCuenta
                {
                    Mensaje = new Mensaje
                    {
                        Codigo = 7,
                        Descripcion = App_Code.Constantes.Mensaje.Mensaje7ErrorPermiteConsulta,
                        ErrorTecnico = null
                    },
                    Respuesta = null
                };

                return objValidaCuenta;
            }*/

            // Valida los datos
            try
            {
                strCuenta = cuenta.Trim();
                bytMoneda = moneda;
                bytTipo = tipocuenta;
                lngBanco = banco;

                // Correcci�n Vulnerabilidades
                try
                {
                    if (!(strCuenta == null))
                    {
                        strCuenta = strCuenta.Replace("[^a-zA-Z0-9]", "");
                    }
                }
                catch (Exception e)
                {
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                        null, 1, App_Code.Constantes.Mensaje.Mensaje1Excepcion + " " + e.ToString());

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 1,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje1Excepcion,
                            ErrorTecnico = e.ToString()
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta;

                }

            }
            catch (Exception e)
            {
                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                    null, 1, App_Code.Constantes.Mensaje.Mensaje1Excepcion + " " + e.ToString());

                // Retorna la excepcion
                ValidaCuenta objValidaCuenta = new ValidaCuenta
                {
                    Mensaje = new Mensaje
                    {
                        Codigo = 1,
                        Descripcion = App_Code.Constantes.Mensaje.Mensaje1Excepcion,
                        ErrorTecnico = e.ToString()
                    },
                    Respuesta = null
                };

                return objValidaCuenta;
            }

            // Obtiene los tokens
            Banca.TokenVirtual_DLL.Negocio.LogicaTokenVirtual objToken = new Banca.TokenVirtual_DLL.Negocio.LogicaTokenVirtual();
            Banca.TokenVirtual_DLL.Comunicacion_WS.Respuesta tokenAsignado = objToken.tieneTokenAsignado(intInstalacion.ToString(), strUsuario, "1");
            Banca.TokenVirtual_DLL.Comunicacion_WS.Respuesta TipoAcceso = objToken.TipoAccesoUsuario(intInstalacion.ToString(), strUsuario);
            bool modeloTokenAsignado = tokenAsignado.Resultado;
            bool modeloTipoAcceso = TipoAcceso.Resultado;

            // Obtiene el tipo de usuario a traves de la funcion
            int respuesta = validaModuloGuateACH(1, intInstalacion, strUsuario, modeloTipoAcceso, modeloTokenAsignado);

            // Revisa las validaciones para retornar no exito
            switch (respuesta)
            {
                case ConstantesACH.C_USUARIO_LIGHT:
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora1 = new App_Code.Bitacora();
                    objBitacora1.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                        null, 1, App_Code.Constantes.Mensaje.Mensaje2ErrorULightFull + " " + App_Code.Constantes.Mensaje.DesErrorUlightFull);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta1 = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 2,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje2ErrorULightFull,
                            ErrorTecnico = App_Code.Constantes.Mensaje.DesErrorUlightFull
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta1;

                case ConstantesACH.C_USUARIO_FULL:
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora2 = new App_Code.Bitacora();
                    objBitacora2.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                        null, 1, App_Code.Constantes.Mensaje.Mensaje2ErrorULightFull + " " + App_Code.Constantes.Mensaje.DesErrorUlightFull);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta2 = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 2,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje2ErrorULightFull,
                            ErrorTecnico = App_Code.Constantes.Mensaje.DesErrorUlightFull
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta2;

                case ConstantesACH.C_USUARIO_CONTRATO:
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora3 = new App_Code.Bitacora();
                    objBitacora3.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                        null, 1, App_Code.Constantes.Mensaje.Mensaje3ErrorUContrato + " " + App_Code.Constantes.Mensaje.DesErrorUContrato);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta3 = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 3,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje3ErrorUContrato,
                            ErrorTecnico = App_Code.Constantes.Mensaje.DesErrorUContrato
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta3;

                case ConstantesACH.C_NO_TIENE_PERMISO:
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora4 = new App_Code.Bitacora();
                    objBitacora4.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                        null, 1, App_Code.Constantes.Mensaje.Mensaje4ErrorUPermiso + " " + App_Code.Constantes.Mensaje.DesErrorPermiso);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta4 = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 4,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje4ErrorUPermiso,
                            ErrorTecnico = App_Code.Constantes.Mensaje.DesErrorPermiso
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta4;

            }

            // Aplica expresiones regulares
            Regex objExpresionRegular = new Regex("\\t+");
            strCuenta = objExpresionRegular.Replace(strCuenta, "");

            ACH_dbBanco.Banco objBanco = new ACH_dbBanco.Banco(); // = Server.createObject("ACH_dbBanco.Banco")
            Recordset rsBanco = objBanco.lstBanco(bytMoneda);
            objBanco = null;

            rsBanco.Filter = ("CodBanco = " + lngBanco);
            ConexionWSCuentaIBAN.CuentaIBAN Conexion = new ConexionWSCuentaIBAN.CuentaIBAN(); // = Server.createObject("ConexionWSCuentaIBAN.CuentaIBAN")

            // Si el tipo de producto es diferente a prestamo o tarjeta
            if (bytTipo != 3 & bytTipo != 4)
            {
                // Da formato a la cuenta
                StrCuentaIBAN = Conexion.ConvertirCuentaBI_CuentaIBAN(strCuenta.Trim(), bytTipo.ToString().Trim(), bytMoneda.ToString().ToString(),
                    rsBanco.Datos.DataSet.Tables[0].Rows[0][4].ToString());

                // corpareyes Inicio [23/01/2019] Validar cuenta IBAN
                if (StrCuentaIBAN.Length < 28 || (strCuenta.Trim().Replace("[0]", "").Length == 0))
                {
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                        null, 1, App_Code.Constantes.Mensaje.Mensaje5ErrorEncontrado);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 5,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje5ErrorEncontrado,
                            ErrorTecnico = null
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta;

                }
                else // Si el tipo de cuenta es  tarjeta o prestamo
                {
                    StrCuentaIBAN = strCuenta.Trim();
                }

                Conexion = null;
            }

            // Genera UUID
            System.Guid generadorID = System.Guid.NewGuid();

            // Obtiene la Url de configuraciones
            var objUrl = Configuraciones.ObtenerValor(Constantes.Configuracion.UrlICG);

            // Si requiere obtener el Token
            if (!blnExisteToken)
            {
                if (strToken == null ^ strToken == "") // Si el token esta vacio o nulo, solicita un nuevo Token
                {
                    try
                    {
                        // Obtiene el Token y valida el resultado
                        strToken = ObtenerToken();
                        if (strToken == null ^ strToken == "")
                        {
                            // Registra evento en Bicatora
                            App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                            objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), null,
                                null, 1, App_Code.Constantes.Mensaje.Mensaje8ErrorToken);

                            // Retorna la excepcion
                            ValidaCuenta objValidaCuenta = new ValidaCuenta
                            {
                                Mensaje = new Mensaje
                                {
                                    Codigo = 8,
                                    Descripcion = App_Code.Constantes.Mensaje.Mensaje8ErrorToken,
                                    ErrorTecnico = null
                                },
                                Respuesta = null
                            };

                            return objValidaCuenta;
                        }
                    }
                    catch (Exception e)
                    {
                        strRespuestaToken = e.ToString();

                        // Registra evento en Bicatora
                        App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                        objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, banco.ToString(), App_Code.Constantes.Configuracion.BancoOrigen.ToString(), strSolicitudToken,
                            strRespuestaToken, 1, App_Code.Constantes.Mensaje.Mensaje23ErrorWS + " " + e.ToString());

                        // Retorna la excepcion
                        ValidaCuenta objValidaCuenta = new ValidaCuenta
                        {
                            Mensaje = new Mensaje
                            {
                                Codigo = 23,
                                Descripcion = App_Code.Constantes.Mensaje.Mensaje23ErrorWS,
                                ErrorTecnico = e.ToString()
                            },
                            Respuesta = null
                        };

                        return objValidaCuenta;
                    }
                }
            }

            // Obtiene los datos del BIC Banco
            Conexion objConexion2 = new Conexion();

            // Recibe los parametros, crea contenedor y parametro
            SpParameters lstParametros2 = new SpParameters();
            SpParameter objParametro2 = new SpParameter();

            objParametro2 = new SpParameter();
            objParametro2.Nombre = "codbanco";
            objParametro2.Tipo = objParametro2.entero;
            objParametro2.Valor = lngBanco.ToString();
            lstParametros2.Add(objParametro2);

            DataSet dsResultado2 = objConexion2.conexion_ConsultaReturnDataSet(App_Code.Constantes.Configuracion.SPBic, lstParametros2);

            // Obtiene el BIC
            string strBICOrigen = dsResultado2.Tables[0].Rows[0][3].ToString();
            string strBICDestino = dsResultado2.Tables[0].Rows[1][3].ToString();

            // Obtiene de las configuraciones el type
            var objTypeSolicitud = Configuraciones.ObtenerValor(Constantes.Configuracion.TypeSolicitud);

            // Arma el json de solicitud
            Solicitud objRequerimiento = new Solicitud
            {
                Destination = strBICDestino,
                type = objTypeSolicitud.ToString(),
                payload = new payload
                {
                    account = StrCuentaIBAN,
                    currency = bytMoneda,
                    product = bytTipo
                },
                Source = strBICOrigen,
                UUID = generadorID.ToString()
            };

            try
            {
                // Define el timeout
                bool blnTimeOut = false;

                // Consulta el servicio de ICG
                Respuesta objRespuesta = new Respuesta();
                objRespuesta = ConsultarCuenta(objRequerimiento, objUrl.ToString(), strToken, ref blnTimeOut);

                // Si supero el tiempo permitido de respuesta
                if (blnTimeOut)
                {
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, strBICDestino, strBICOrigen, strJsonSolicitud,
                        strJsonRespuesta, 1, App_Code.Constantes.Mensaje.Mensaje9ErrorTiempo);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 9,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje9ErrorTiempo,
                            ErrorTecnico = null
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta;
                }

                // Obtiene el numero de intentos
                var objIntentos = Configuraciones.ObtenerValor(Constantes.Configuracion.IntentosToken);

                // Valida que la respuesta del servicio de ICG sea 200 Ok
                int intVeces = 0;
                do
                {
                    if (strRespuestaServicio != "OK")
                    {
                        // Vuelve a obtener Token
                        strToken = ObtenerToken();

                        // Vuelve a ejecutar 
                        objRespuesta = ConsultarCuenta(objRequerimiento, objUrl.ToString(), strToken, ref blnTimeOut);

                        // Si supero el tiempo permitido de respuesta
                        if (blnTimeOut)
                        {
                            // Registra evento en Bicatora
                            App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                            objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, strBICDestino, strBICOrigen, strJsonSolicitud,
                                strJsonRespuesta, 1, App_Code.Constantes.Mensaje.Mensaje9ErrorTiempo);

                            // Retorna la excepcion
                            ValidaCuenta objValidaCuenta = new ValidaCuenta
                            {
                                Mensaje = new Mensaje
                                {
                                    Codigo = 9,
                                    Descripcion = App_Code.Constantes.Mensaje.Mensaje9ErrorTiempo,
                                    ErrorTecnico = null
                                },
                                Respuesta = null
                            };

                            return objValidaCuenta;
                        }

                        intVeces++;
                    }
                    else { break; }
                } while (intVeces < Convert.ToInt32(objIntentos));

                
                // Valida la integridad del UUID
                if (objRespuesta.UUID != generadorID.ToString().ToUpper())
                {
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, strBICDestino, strBICOrigen, strJsonSolicitud,
                        strJsonRespuesta, 1, App_Code.Constantes.Mensaje.Mensaje7ErrorUuid);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 7,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje7ErrorUuid,
                            ErrorTecnico = null
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta;
                }

                // Valida que la cuenta exista en el banco destino
                if (objRespuesta.payload.found == "true")
                {
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, strBICDestino, strBICOrigen, strJsonSolicitud,
                        strJsonRespuesta, 0, App_Code.Constantes.Mensaje.Mensaje0Ok);

                    // Retorna mensaje de exito
                    ValidaCuenta objValidaCuenta = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 0,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje0Ok,
                            ErrorTecnico = null
                        },
                        Respuesta = new payloadR
                        {
                            found = objRespuesta.payload.found,
                            name = objRespuesta.payload.name
                        }
                    };

                    return objValidaCuenta;
                }
                else
                {
                    // Registra evento en Bicatora
                    App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                    objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, strBICDestino,strBICOrigen, strJsonSolicitud,
                        strJsonRespuesta.ToString(), 1, App_Code.Constantes.Mensaje.Mensaje5ErrorEncontrado);

                    // Retorna la excepcion
                    ValidaCuenta objValidaCuenta = new ValidaCuenta
                    {
                        Mensaje = new Mensaje
                        {
                            Codigo = 6,
                            Descripcion = App_Code.Constantes.Mensaje.Mensaje5ErrorEncontrado,
                            ErrorTecnico = null
                        },
                        Respuesta = null
                    };

                    return objValidaCuenta;
                }

            }
            catch (Exception e)
            {
                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(bytCanal, intInstalacion, strUsuario, strBICDestino, strBICOrigen, strJsonSolicitud,
                    null, 1, App_Code.Constantes.Mensaje.Mensaje23ErrorWS + " " + e.ToString());

                // Retorna la excepcion
                ValidaCuenta objValidaCuenta = new ValidaCuenta
                {
                    Mensaje = new Mensaje
                    {
                        Codigo = 23,
                        Descripcion = App_Code.Constantes.Mensaje.Mensaje23ErrorWS,
                        ErrorTecnico = e.ToString()
                    },
                    Respuesta = null
                };

                return objValidaCuenta;
            }
        }

        // Funcion para validar el token, si esta en blanco, diferente o expirado
        public bool TokenValido(string strToken, ref MensajeError objMensajeError)
        {
            // Obtiene el Token de ICG
            if (strToken == "")
            {
                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(0, 0, null, null, null,
                    strToken, null, 1,
                    App_Code.Constantes.Mensaje.Mensaje19ErrorTokenenBlanco);

                // Llena el mensaje de error
                objMensajeError.error = "invalid_request";
                objMensajeError.error_description = "Invalid Token";

                return false;
            }

            // Valida el Token
            if (ValidarToken(4, ref strToken, strToken) == false & strEstadoToken == "Expirado")
            {
                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(0, 0, null, null, null,
                    strToken, null, 1,
                    App_Code.Constantes.Mensaje.Mensaje20ErrorTokenExpirado);

                // Llena el mensaje de error
                objMensajeError.error = "invalid_request";
                objMensajeError.error_description = "Expired Token";

                return false;
            }

            if (ValidarToken(4, ref strToken, strToken) == false & strEstadoToken == "Diferente")
            {
                // Registra evento en Bicatora
                App_Code.Bitacora objBitacora = new App_Code.Bitacora();
                objBitacora.RegistrarBitacora(0, 0, null, null, null,
                    strToken, null, 1,
                    App_Code.Constantes.Mensaje.Mensaje21ErrorTokenDiferente);

                // Llena el mensaje de error
                objMensajeError.error = "invalid_request";
                objMensajeError.error_description = "Different Token";

                return false;
            }

            // Posterior a las validaciones el Token es verdadero
            return true;
        }

        // Funcion heredada de VB
        private int verificaSituacionCuentaXMoneda(string pCuenta, int pMoneda, int pTipoCuenta, ref bool blnEncontrado, ref string strDescripcion, string pTipoBanco="")
        {
            int status;
            int moneda;
            string strStatus;
            //objCons = new clsConstantes();
            string strRes;
            DataSet ds;
            switch (pTipoCuenta)
            {
                case Constantes.VB.C_CTA_MONETARIOS:
                    try
                    {
                        pCuenta = strVerificaCadenaCuenta(pCuenta, Constantes.VB.C_CTA_MONETARIOS);
                        ADOR.Recordset myRcd = new ADOR.Recordset();
                        myRcd = this.VerificaSituacionCuentaNativo(pCuenta, Constantes.VB.C_CTA_MONETARIOS.ToString());
                        myRcd.MoveFirst();
                        status = Convert.ToInt32(myRcd.Datos.Rows[0]["DFH_SITUACION"]);
                        moneda = Convert.ToInt32(myRcd.Datos.Rows[0]["DFH_CODIGO_MONEDA"]);
                        strNombreCuenta = myRcd.Datos.Rows[0]["DFH_NOMBRE"].ToString().Trim();
                        //status = ((int)(myRcd.Datos.Rows[0]["DFH_SITUACION"]));
                        //moneda = ((int)(myRcd.Datos.Rows[0]["DFH_CODIGO_MONEDA"]));
                        if (!(pMoneda == moneda))
                        {
                            status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                            blnEncontrado = false;
                            strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                        }
                        else
                        {
                            switch (status)
                            {
                                case 5:
                                    status = Constantes.VB.C_EDO_CTA_INACTIVA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje25ErrorCuentaInactiva;
                                    break;
                                case 3:
                                    status = Constantes.VB.C_EDO_CTA_BLOQUEADA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje26ErrorCuentaBloqueada;
                                    break;
                                case 4:
                                    status = Constantes.VB.C_EDO_CTA_CANCELADA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje27ErrorCuentaCancelada;
                                    break;
                                case 2:
                                    status = Constantes.VB.C_EDO_CTA_EMBARGADA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje28ErrorCuentaEmbargada;
                                    break;
                                case 1:
                                    status = Constantes.VB.C_ESTADO_CTA_VIGENTE;
                                    blnEncontrado = true;
                                    strDescripcion = Constantes.Mensaje.Mensaje24CuentaVigente;
                                    break;
                                default:
                                    status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                                    break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                        blnEncontrado = false;
                        strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida + " " + ex.ToString(); 
                    }

                    break;
                case Constantes.VB.C_CTA_AHORROS:
                    try
                    {
                        pCuenta = strVerificaCadenaCuenta(pCuenta, Constantes.VB.C_CTA_AHORROS);
                        ADOR.Recordset myRcd = new ADOR.Recordset();
                        myRcd = this.VerificaSituacionCuentaNativo(pCuenta, Constantes.VB.C_CTA_AHORROS.ToString());
                        myRcd.MoveFirst();
                        status = Convert.ToInt32(myRcd.Datos.Rows[0]["DFH_SITUACION"]);
                        moneda = Convert.ToInt32(myRcd.Datos.Rows[0]["DFH_CODIGO_MONEDA"]);
                        strNombreCuenta = myRcd.Datos.Rows[0]["DFH_NOMBRE"].ToString().Trim();
                        //status = ((int)(myRcd.Datos.Rows[0]["DFH_SITUACION"]));
                        //moneda = ((int)(myRcd.Datos.Rows[0]["DFH_CODIGO_MONEDA"]));
                        if (!(pMoneda == moneda))
                        {
                            status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                            blnEncontrado = false;
                            strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                        }
                        else
                        {
                            switch (status)
                            {
                                case 1:
                                    status = Constantes.VB.C_ESTADO_CTA_VIGENTE;
                                    blnEncontrado = true;
                                    strDescripcion = Constantes.Mensaje.Mensaje24CuentaVigente;
                                    break;
                                case 5:
                                    status = Constantes.VB.C_EDO_CTA_BLOQUEADA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje26ErrorCuentaBloqueada;
                                    break;
                                case 2:
                                    status = Constantes.VB.C_EDO_CTA_CANCELADA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje27ErrorCuentaCancelada;
                                    break;
                                case 4:
                                    status = Constantes.VB.C_EDO_CTA_EMBARGADA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje28ErrorCuentaEmbargada;
                                    break;
                                default:
                                    status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                                    break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                        blnEncontrado = false;
                        strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                    }

                    break;
                case Constantes.VB.C_CTA_TARJETA_CREDITO:
                    try
                    {
                        pCuenta = strVerificaCadenaCuenta(pCuenta, Constantes.VB.C_CTA_TARJETA_CREDITO);
                        ADOR.Recordset myRcd = new ADOR.Recordset();
                        myRcd = this.VerificaSituacionCuentaNativo(pCuenta, "4");
                        myRcd.MoveFirst();
                        status = Convert.ToInt32(myRcd.Datos.Rows[0]["DFH_SITUACION"]);
                        moneda = Convert.ToInt32(myRcd.Datos.Rows[0]["DFH_CODIGO_MONEDA"]);
                        strNombreCuenta = myRcd.Datos.Rows[0]["DFH_NOMBRE"].ToString().Trim();
                        //status = ((int)(myRcd.Datos.Rows[0]["DFH_SITUACION"]));
                        //moneda = ((int)(myRcd.Datos.Rows[0]["DFH_CODIGO_MONEDA"]));
                        if ((moneda == 2))
                        {
                            switch (status)
                            {
                                case 1:
                                case 2:
                                case 4:
                                case 5:
                                case 6:
                                case 7:
                                case 8:
                                case 10:
                                case 11:
                                case 12:
                                case 13:
                                case 14:
                                    status = Constantes.VB.C_ESTADO_CTA_VIGENTE;
                                    blnEncontrado = true;
                                    strDescripcion = Constantes.Mensaje.Mensaje24CuentaVigente;
                                    break;
                                default:
                                    status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                                    blnEncontrado = false;
                                    strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                                    break;
                            }
                        }
                        else if ((moneda == 1))
                        {
                            if ((pMoneda == moneda))
                            {
                                // '' Esto quiere decir que solo acepta saldo en quetzales                         
                                switch (status)
                                {
                                    case 1:
                                    case 2:
                                    case 4:
                                    case 5:
                                    case 6:
                                    case 7:
                                    case 8:
                                    case 10:
                                    case 11:
                                    case 12:
                                    case 13:
                                    case 14:
                                        status = Constantes.VB.C_ESTADO_CTA_VIGENTE;
                                        blnEncontrado = true;
                                        strDescripcion = Constantes.Mensaje.Mensaje24CuentaVigente;
                                        break;
                                    default:
                                        status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                                        blnEncontrado = false;
                                        strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                                        break;
                                }
                            }
                            else
                            {
                                status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                                blnEncontrado = false;
                                strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                            }

                        }
                        else
                        {
                            status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                            blnEncontrado = false;
                            strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                        }

                    }
                    catch (Exception ex)
                    {
                        status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                        blnEncontrado = false;
                        strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                    }

                    break;
                case Constantes.VB.C_CTA_PRESTAMOS:
                    try
                    {
                        BM_OperacionesCuenta.clsConsultaSaldo objCuenta = new BM_OperacionesCuenta.clsConsultaSaldo();
                        //BM_OperacionesCuenta.clsConsultaSaldo objCuenta = new BM_OperacionesCuenta.clsConsultaSaldo();
                        strRes = objCuenta.PrestamosAs400(pCuenta, pMoneda.ToString());
                        ds = objCuenta.dsResultadoConsulta();
                        if (ds.Tables[0].Rows[0]["Moneda"].Equals(pMoneda.ToString()))
                        {
                            if (ds.Tables[0].Rows[0]["DescripcionEstado"].ToString().Equals(Constantes.VB.C_ESTADO_CTA_PRESTAMOS_VIGENTE))
                            {
                                // Public C_ESTADO_CTA_PRESTAMOS_VIGENTE As String = "VIGENTE AL DIA" 
                                status = Constantes.VB.C_EDO_CTA_VIGENTE;
                                blnEncontrado = true;
                                strDescripcion = Constantes.Mensaje.Mensaje24CuentaVigente;

                            }
                            else
                            {
                                status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                                blnEncontrado = false;
                                strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                            }
                        }
                        else
                        {
                            status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                            blnEncontrado = false;
                            strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                        }

                    }
                    catch (Exception ex)
                    {
                        status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                        blnEncontrado = false;
                        strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                    }

                    break;
                default:
                    status = Constantes.VB.C_EDO_CTA_DESCONOCIDA;
                    blnEncontrado = false;
                    strDescripcion = Constantes.Mensaje.Mensaje29ErrorCuentaDesconocida;
                    break;
            }
            int intResultadoVerificacionCuenta = status;
            return  status;
        }

        // Funcion heredada de VB
        private string strVerificaCadenaCuenta (string pCuenta, int pTipoCuenta)
        {
            //clsConstantes cons = new clsConstantes();
            string strCuenta;
            string strTemp2;
            strCuenta = pCuenta;
            // Trim(Double.Parse(pCuenta).ToString())
            switch (pTipoCuenta)
            {
                case Constantes.VB.C_CTA_MONETARIOS:
                    // Para cuentas de Monetarios
                    // 1.Solo son validas las cuentas de 10 digitos
                    // Inicio corplaa 2012 WTACH
                    if ((strCuenta.Length > 10))
                    {
                        strCuenta = strCuenta.Substring((strCuenta.Length - 10), 10);
                    }

                    if (!((strCuenta.Length == 10)
                                || (strCuenta.Length == 9)))
                    {
                        strCuenta = "0000000000";
                    }

                    break;
                case Constantes.VB.C_CTA_AHORROS:
                    // ************************************* ealopez 20080827 ********************************************************
                    // Para cuentas de ahorro
                    // 1. solo son validas las cuentas de 10 y 7 digitos
                    // Inicio corplaa 2012 WTACH
                    if ((strCuenta.Length >= 10))
                    {
                        strCuenta = strCuenta.Substring((strCuenta.Length - 7), 7);
                    }

                    // ************************************* ealopez 20080827 ********************************************************
                    break;
                case Constantes.VB.C_CTA_TARJETA_CREDITO:
                    if ((strCuenta.Length > 16))
                    {
                        // Caso: 000000000000001234
                        strTemp2 = strCuenta.Substring(0, (strCuenta.Length - 16));
                        // '' Los caracteres de relleno de la cuenta
                        strCuenta = strCuenta.Substring((strCuenta.Length - 16), 16);
                        // '' Cuenta a 0000001234
                        try
                        {
                            if ((int.Parse(strTemp2) > 0))
                            {
                                // '' Error: ya que debe de venir 0
                                strCuenta = "0000000";
                            }

                        }
                        catch (Exception e)
                        {
                            // Registra en Bitacora de Aplicacion
                            BitacoraAplicacion.RegistrarError(e.ToString());

                            strCuenta = "0000000";
                        }
                    }
                    else if ((strCuenta.Length < 16))
                    {
                        strCuenta = "0000000";
                    }

                    break;
                default:
                    strCuenta = "0000000";
                    break;
            }
            return strCuenta;
        }

        // Funcion heredada de VB
        private Recordset VerificaSituacionCuentaNativo(string pCuenta, string pTipoCuenta)
        {
            int i = 0;
            ADOR.Recordset rsCuenta390;
            BibliotecaBEL_390_InfoGralCta.CLInfoGralCta obj390DatosCuenta = new BibliotecaBEL_390_InfoGralCta.CLInfoGralCta();
            try
            {
                rsCuenta390 = obj390DatosCuenta.NewRecordset("DFH_ARREGLO");

                string[] varfields = { "DFH_TIPO_CUENTA", "DFH_CUENTA", "DFH_NOMBRE", "DFH_SITUACION", "DFH_CODIGO_MONEDA", "DFH_ORIGEN", "DFH_CODRET" };
                object[] varvalues = { int.Parse(pTipoCuenta), pCuenta.Trim(), "----------------------------------------", 0,0,0,0};
                rsCuenta390.AddNew(varfields, varvalues);

                do
                {
                    object[] varvaluesd = { 0, "00000000000", "----------------------------------------" ,0,0,0,0};
                    rsCuenta390.AddNew(varfields, varvaluesd);

                    i = (i + 1);
                } while ((i != 19));

                object objRsCuenta390 = rsCuenta390;

                obj390DatosCuenta.Baad3qlv(ref objRsCuenta390);

                return rsCuenta390 = (ADOR.Recordset)objRsCuenta390;
            }
            catch (Exception e)
            {
                // Registra en Bitacora de Aplicacion
                BitacoraAplicacion.RegistrarError(e.ToString());

                return null;
            }

        }

        // Funcion que valida el token, que sea el mismo y si no esta vencido
        // Tipo 3 Token que guardamos para consultar cuentas a otros bancos 
        // Tipo 4 Token que entregamos a ICG para las operaciones
        private bool ValidarToken(int intTipo, ref string strSiToken, string strToken = "")
        {
            // Obtiene el factor tiempo de configuraciones
            var objFactorTiempo = Configuraciones.ObtenerValor(Constantes.Configuracion.FactorTiempo);

            // Obtiene los datos del token
            Conexion objConexion = new Conexion();

            // Obtiene el Token almacenado
            SpParameters lstParametros = new SpParameters();
            SpParameter objParametro = new SpParameter();

            objParametro = new SpParameter();
            objParametro.Nombre = "operacion";
            objParametro.Tipo = objParametro.entero;
            objParametro.Valor = intTipo.ToString();
            lstParametros.Add(objParametro);

            DataSet dsResultado = objConexion.conexion_ConsultaReturnDataSet(App_Code.Constantes.Configuracion.SPToken, lstParametros);

            // Obtiene el tiempo 
           double dblTiempoAccessToken = 0;
            switch (Convert.ToInt32(objFactorTiempo))
            {
                case 1: // minutos
                    dblTiempoAccessToken = Convert.ToDouble(dsResultado.Tables[0].Rows[0][2]) / 60;
                    break;
                case 2: // horas
                    dblTiempoAccessToken = Convert.ToDouble(dsResultado.Tables[0].Rows[0][2]) /3600;
                    break;
                case 3: // dias
                    dblTiempoAccessToken = Convert.ToDouble(dsResultado.Tables[0].Rows[0][2]) /86400;
                    break;
            }

            // Si es token que guarda BI
            if (intTipo == 3)
            {
                // Valida que no haya expirado
                DateTime fecToken = Convert.ToDateTime(dsResultado.Tables[0].Rows[0][6]); // Obtiene la fecha / hora de registro del Token
                DateTime fecActual = Convert.ToDateTime(dsResultado.Tables[0].Rows[0][7]); // Obtiene la fecha actual desde el servidor DB

                var objTiempo = fecActual - fecToken; // Obtiene el tiempo que ha transcurrido desde que el Token se obtuvo

                // Valida que sea el mismo dia
                if (fecActual.Date == fecToken.Date)
                {
                    // Si los minutos que han transcurrido son mayores a la expiracion del token devuelve false
                    if (objTiempo.TotalHours >= dblTiempoAccessToken)
                    {
                        strEstadoToken = "Expirado";
                        return false;
                    }
                    else // Si el token no ha expirado el Token es valido
                    {
                        strSiToken = dsResultado.Tables[0].Rows[0][1].ToString();
                        return true;
                    } // Si el token no ha expirado el Token es valido
                }
                else // Si no es el mismo dia
                {
                    strEstadoToken = "Expirado";
                    return false;
                }
            }
            if (intTipo == 4) // Si es token que entregamos a la ICG
            {
                // Valida que sea el mismo Token
                if (dsResultado.Tables[0].Rows[0][1].ToString() == strToken.Trim())
                {
                    // Valida que no haya expirado
                    DateTime fecToken = Convert.ToDateTime(dsResultado.Tables[0].Rows[0][6]); // Obtiene la fecha / hora de registro del Token
                    DateTime fecActual = Convert.ToDateTime(dsResultado.Tables[0].Rows[0][7]); // Obtiene la fecha actual desde el servidor DB

                    var objTiempo = fecActual - fecToken; // Obtiene el tiempo que ha transcurrido desde que el Token se obtuvo

                    // Valida que sea el mismo dia
                    if (fecActual.Date == fecToken.Date)
                    {
                        // Si los minutos que han transcurrido son mayores a la expiracion del token devuelve false
                        if (objTiempo.TotalHours >= dblTiempoAccessToken)
                        {
                            strEstadoToken = "Expirado";
                            return false;
                        }
                        else { return true; } // Si el token es el mismo y no ha expirado el Token es valido
                    }
                    else // Si no es el mismo dia
                    {
                        strEstadoToken = "Expirado";
                        return false;
                    }
                }
                else
                {
                    strEstadoToken = "Diferente";
                    return false;
                }
            }

            return false;
        }

        // Funcion para dar formato al nombre
        private string DarFormatoNombre(string strNombre)
        {
            // valida el Nombre
            if (strNombre != "" || strNombre != null)
            {
                int intLongitud = strNombre.Length;
                strNombre = strNombre.Substring(0, 3) + "**********" + strNombre.Substring((intLongitud - 3), 3);
            }
            return strNombre;
        }
        
        // Funcion que verifica si una cadena se puede convertir a numero
        private bool IsNumeric(string strCadena)
        {
            // Intenta convertir una cadena en numero
            try
            {
                int result = int.Parse(strCadena);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Funcion para generar el mensaje Json de respuesta
        private Respuesta GenerarMensajeRespuesta(string strDestination, string strType, string strFound, string strName, string strSource, string strUuid)
        {
            // Arma el mensaje de respuesta
            Respuesta objRespuesta = new Respuesta
            {
                Destination = strDestination,
                type = strType,
                payload = new payloadR
                {
                    found = strFound,
                    name = strName
                },
                Source = strSource,
                UUID = strUuid
            };

            return objRespuesta;
        }

        // Funcion para hacer la consulta al servicio de ICG y validar la cuenta
        private Respuesta ConsultarCuenta(Solicitud objSolicitud, string strUrl, string strToken, ref bool blnTimeOut)
        {
            // Define la variable para controlar el tiempo de ejecucion
            bool Completado;

            // Obtiene el factor tiempo de configuraciones
            var objTimeOut = Configuraciones.ObtenerValor(Constantes.Configuracion.TimeOut);

            // Consume el API REST de ICG Valida Cuenta
            var json = JsonConvert.SerializeObject(objSolicitud);
            string responseBody = "";

            var request = (HttpWebRequest)WebRequest.Create(strUrl);
            request.Method = "POST";

            // Agrega los datos de la cabecera y el Token
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + strToken);
            request.ContentType = "application/json";
            request.Accept = "application/json";

            // Intenta ejecutar la llamada al proceso en el tiempo configurado
            Completado = ExecuteWithTimeLimit(TimeSpan.FromSeconds(Convert.ToInt32(objTimeOut)), () =>
            {

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                using (WebResponse response = request.GetResponse())
                {
                    // Obtiene la respuesta del servicio
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)request.GetResponse();
                    strRespuestaServicio = myHttpWebResponse.StatusCode.ToString();

                    using (Stream strReader = response.GetResponseStream())
                    {
                        using (StreamReader objReader = new StreamReader(strReader))
                        {
                            responseBody = objReader.ReadToEnd();

                            // Simula timeout
                            //int milliseconds = 20000;
                            //Thread.Sleep(milliseconds);

                            // Asigna valores a las propiedades
                            strJsonSolicitud = json.ToString();
                            strJsonRespuesta = responseBody;
                        }
                    }
                }
            });

            // Si el bloque de codigo supero el tiempo configurado
            if (!Completado)
            {
                blnTimeOut = true;
                return null;
            }
            else { return JsonConvert.DeserializeObject<Respuesta>(responseBody); } // Si no hay timeout devuelve la respuesta del servicio

        }

        // Funcion que obtiene un Token
        private string ObtenerToken()
        {
            // Obtiene las configuraciones relacionadas al Token
            var objUrlToken = Configuraciones.ObtenerValor(Constantes.Configuracion.UrlToken);
            var objTokenName = Configuraciones.ObtenerValor(Constantes.Configuracion.TokenName);
            var objAccessTokenUrl = Configuraciones.ObtenerValor(Constantes.Configuracion.AccessTokenUrl);
            var objUsuarioToken = Configuraciones.ObtenerValor(Constantes.Configuracion.UsuarioToken);
            var objPassword = Configuraciones.ObtenerValor(Constantes.Configuracion.Password);
            var objClientID = Configuraciones.ObtenerValor(Constantes.Configuracion.ClientID);
            var objClientSecret = Configuraciones.ObtenerValor(Constantes.Configuracion.ClientSecret);
            var objScope = Configuraciones.ObtenerValor(Constantes.Configuracion.Scope);

            // Consume el API REST de ICG Token
            string responseBody = "";

            var url = "?tokenname=" + objTokenName.ToString() + "&username=" + objUsuarioToken.ToString() + "&password=";
            url += objPassword.ToString() + "&client_id=" + objClientID.ToString() + "&client_secret=" + objClientSecret.ToString() + "&scope=" + objScope.ToString() + "&grant_type=" + objAccessTokenUrl.ToString();

            strSolicitudToken = url.ToString();

            var request = (HttpWebRequest)WebRequest.Create(objUrlToken.ToString());
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                //streamWriter.Write(json);
                streamWriter.Write(url);
                streamWriter.Flush();
                streamWriter.Close();
            }

            using (WebResponse response = request.GetResponse())
            {
                using (Stream strReader = response.GetResponseStream())
                {
                    using (StreamReader objReader = new StreamReader(strReader))
                    {
                        responseBody = objReader.ReadToEnd();

                        // Almacena y retorna el AccessToken
                        return GuardarToken(responseBody);
                    
                    }
                }
            }
        }

        // Funcion para guardar el token en DB
        private string GuardarToken(string strToken)
        {
            // Convierte el resultado en un objeto
            Token objToken = JsonConvert.DeserializeObject<Token>(strToken);

            // Guarda en DB
            // Obtiene los datos del token
            Conexion objConexion = new Conexion();

            // Obtiene el Token almacenado
            SpParameters lstParametros = new SpParameters();
            SpParameter objParametro = new SpParameter();

            objParametro = new SpParameter();
            objParametro.Nombre = "token";
            objParametro.Tipo = objParametro.cadena;
            objParametro.Valor = objToken.access_token;
            lstParametros.Add(objParametro);

            objParametro = new SpParameter();
            objParametro.Nombre = "vigenciatoken";
            objParametro.Tipo = objParametro.entero;
            objParametro.Valor = objToken.expires_in.ToString();
            lstParametros.Add(objParametro);

            objParametro = new SpParameter();
            objParametro.Nombre = "refreshtoken";
            objParametro.Tipo = objParametro.cadena;
            objParametro.Valor = objToken.refresh_token;
            lstParametros.Add(objParametro);

            objParametro = new SpParameter();
            objParametro.Nombre = "vigenciarefreshtoken";
            objParametro.Tipo = objParametro.entero;
            objParametro.Valor = objToken.refresh_expires_in.ToString();
            lstParametros.Add(objParametro);

            objParametro = new SpParameter();
            objParametro.Nombre = "operacion";
            objParametro.Tipo = objParametro.entero;
            objParametro.Valor = "1";
            lstParametros.Add(objParametro);

            if (objConexion.conexion_EjecutarSP(Constantes.Configuracion.SPToken, lstParametros))
            {
                return objToken.access_token;
            }
            else
            {
                return null;
            }
        }

        // Funcion que maneja hilos de proceso para control de tiempo en bloques de codigo
        public static bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock)
        {
            try
            {
                Task task = Task.Factory.StartNew(() => codeBlock());
                task.Wait(timeSpan);
                return task.IsCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }
        }

        // Funcion trasladada desde el Include Banca.Servicios.TransferenciasACH
        private int validaModuloGuateACH(int intOrigen, int sInstalacion, string sUsuario, bool TipoAcceso, bool TokenAsignado)
        {
            int respuesta = 0;
            //object blnAceptaContrato;
            bool blnAceptaContrato = false;
            // Dim objUsuarioCli
            // Dim objbsCliente
            // Dim objInstalacion
            // Dim rsInstalacion
            //object blnValidaRedireccion;
            bool blnValidaRedireccion;
            blnValidaRedireccion = false;
            //object blnPerfil;
            //object blnUsrAnt;
            bool blnPerfil;
            bool blnUsrAnt;
            BM_dbPerfilUsuarioCli.PerfilUsuarioCli obj_bsPerfil = new BM_dbPerfilUsuarioCli.PerfilUsuarioCli();
            // Server.CreateObject("BM_dbPerfilUsuarioCli.PerfilUsuarioCli")
            blnPerfil = obj_bsPerfil.ExistePerfil(sInstalacion, sUsuario);
            obj_bsPerfil = null;
            BM_bsUsuariocli.Usuariocli obj_bsUsuariocli = new BM_bsUsuariocli.Usuariocli();

            // = Server.CreateObject("BM_bsUsuariocli.Usuariocli")
            blnUsrAnt = obj_bsUsuariocli.esUsuarioAntiguo(sInstalacion, sUsuario);
            obj_bsUsuariocli = null;
            if (!blnPerfil ^ blnUsrAnt)
            {
                // Select Case intOrigen
                //     Case 1 : session("redirect") = "../ach/transferenciasACH/frmachmnt.asp"
                //     Case 2 : session("redirect") = "../ach/transferenciasACH/frmachtrn.asp"
                //     Case 3 : session("redirect") = "../ach/transferenciasACH/frmachrpt.asp"
                // End Select
                // session("ordenPerfil") = 1
                // Response.Redirect "../../servicios/srvacdslcpaso1.asp?ant=" & blnUsrAnt '?pregResp=" & blnAsignado & "&perfil="& blnPerfil & "&ant=" & blnUsuarioAnt    
                // Redireccionar -> D:\Banca Total\Web\BancaTotal\servicios\srvacdslcpaso1.asp
                respuesta = ConstantesACH.C_USUARIO_VIEJO;
            }

            // [corpjvelasquez][valida tipo acceso, si es full redirecciona al contrato][28042017]
            if (TipoAcceso == false & intOrigen == 1)
            {
                // Response.Redirect "../../app/Token_Virtual/Banca.TokenError/BienLinea/Light/light_Ach.html"
                respuesta = ConstantesACH.C_USUARIO_LIGHT;
            }
            else
            {
                // verificar si ha aceptado el contrato del modulo guate-ACH
                BM_dbUsuariocli.Usuariocli objUsuarioCli = new BM_dbUsuariocli.Usuariocli();

                // = Server.CreateObject("BM_dbUsuarioCli.UsuarioCli")
                blnAceptaContrato = objUsuarioCli.getAceptaGuateACH(sInstalacion, sUsuario);
                objUsuarioCli = null;
                blnValidaRedireccion = true;
            }

            BM_dbInstalacion.Instalacion objInstalacion = new BM_dbInstalacion.Instalacion();
            // = server.CreateObject("BM_dbInstalacion.Instalacion")
            //object rsInstalacion = objInstalacion.BuscaInstalacionXCodigo(sInstalacion);
            Recordset rsInstalacion = objInstalacion.BuscaInstalacionXCodigo(sInstalacion);
            objInstalacion = null;
            //object lngCliente = long.Parse(rsInstalacion.Fields("Cliente").Value);
            object lngCliente = Convert.ToInt64(rsInstalacion.Datos.DataSet.Tables[0].Rows[0][1]);
            rsInstalacion = null;
            if (TokenAsignado == false & intOrigen == 1)
            {
                // Response.Redirect "../../app/Token_Virtual/Banca.TokenError/BienLinea/FullsinToken/FullsinToken_Ach.html"
                respuesta = ConstantesACH.C_USUARIO_FULL;
            }

            BM_bsCliente.Cliente objbsCliente = new BM_bsCliente.Cliente();
            // = server.CreateObject("BM_bsCliente.cliente")
            // <corpjaguilar><01062017><Se hace la redireccion cuando haya hecho las validaciones del token>
            if (blnValidaRedireccion == true)
            {
                if (!blnAceptaContrato)
                {
                    respuesta = ConstantesACH.C_USUARIO_CONTRATO;
                    // response.redirect "frmachctr.asp"
                }
                else
                {
                    // verificar si el cliente tiene  el modulo de guate-ACH
                    if (!objbsCliente.TieneServicio(Convert.ToInt32(lngCliente), 24))
                    {
                        // Call terminarConMensajeError("No tiene permisos para utilizar esta opci�n ", "Para mayor informaci�n comuniquese con Banca Moderna al PBX: 24203200")
                        respuesta = ConstantesACH.C_NO_TIENE_PERMISO;
                    }

                }

            }
            else
            {
                // verificar si el cliente tiene  el modulo de guate-ACH
                if (!objbsCliente.TieneServicio(Convert.ToInt32(lngCliente), 24))
                {
                    // Call terminarConMensajeError("No tiene permisos para utilizar esta opci�n ", "Para mayor informaci�n comuniquese con Banca Moderna al PBX: 24203200")
                    respuesta = ConstantesACH.C_NO_TIENE_PERMISO;
                }

            }

            return respuesta;
        }
    }
}