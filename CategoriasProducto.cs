        // Coloca en una lista todo el contenido de ProductosTracking
        public CategoriasProducto ObtenerCategoriasProducto(int intInstalacion, string strUsuario)
        {
            // Constantes a nivel del metodo
            string strServicio = "WSGestion";
            string strOperacion = "ProductosTracking";

            // Obtiene el tipo de Usuario
            UsuarioPropiedades datUsuario = new UsuarioPropiedades();
            Usuario objUsuario = new Usuario();
            datUsuario = objUsuario.ObtenerDatos(intInstalacion, strUsuario);
            // La propiedad tipo devuelve FULL o LIGHT, realizar la validacion

            // Obtiene la Url de CRM y el Path de las imagenes a partir de las configuraciones
            string strUrl = "";
            string strPathImagenes = "";
            try
            {
                strUrl = Configuraciones.ObtenerValor<string>("TrackingGestiones");
                strPathImagenes = Configuraciones.ObtenerValor<string>("TrackingPathImagenes");
            }
            catch (Exception e)
            {
                // Si la configuracion para obtener el Url es invalida inserta en Bitacora el error
                Bitacora objBitacora = new Bitacora();
                Bitacora.BitacoraModelo datBitacora = new Bitacora.BitacoraModelo();

                datBitacora.Servicio = strServicio;
                datBitacora.Operacion = strOperacion;
                datBitacora.Instalacion = intInstalacion;
                datBitacora.Usuario = strUsuario;
                datBitacora.Producto = null;
                datBitacora.Descripcion = "La configuracion para obtener la Url no es valida " + e.ToString();
                datBitacora.CodRetornoServicio = null;
                datBitacora.DescripcionRetornoServicio = "no hay descripcion de retorno asociada al servicio";
                datBitacora.OrigenError = "CONFIG";
                datBitacora.XMLSolicitud = "No hay XML de solicitud asociado";
                datBitacora.XMLRespuesta = "No hay XML de retorno asociado";
                datBitacora.NodoWeb = null;
                datBitacora.RegistraError = 1;

                objBitacora.InsertarBitacora(datBitacora);

                // Agrega el retorno del mensaje de error
                Mensaje msjError = new Mensaje()
                {
                    Codigo = 2,
                    Nombre = "Error al obtener las configuraciones",
                    ErrorTecnico = e.ToString()
                };

                CategoriasProducto lstCategoriaGestion = new CategoriasProducto
                {
                    Mensaje = msjError,
                    ProductoImagen = null
                };

                return lstCategoriaGestion;
            }

            // Utiliza la clase para obtener los datos del WS - Metodo ProductosTracking
            WebService objWs = new WebService(strUrl, "e_solutions.e_manager." + strServicio + "/", strOperacion, true);

            // Invoca el servicio
            objWs.Invoke();

            // Si la operacion de invocar el WS es valida
            if (objWs.Result)
            {
                // Traslada el resultado a un string
                string strXml = objWs.ResultString;

                // Se crea el nuevo mapeo a partir de la clase DatosProductos, elemento Root del Xml original estructura_productos
                XmlSerializer objSerializador = new XmlSerializer(typeof(List<DatosProductos>), new XmlRootAttribute("estructura_productos"));

                // Lee el string del XMl
                StringReader strReader = new StringReader(strXml);

                List<DatosProductos> lstProductos = null;

                try
                {
                    // Carga el resultado en una lista de la clase Categorias, operacion exitosa
                    lstProductos = (List<DatosProductos>)objSerializador.Deserialize(strReader);

                    // Si la respuesta del mensaje es 1, exitoso; entonces devolvera el contenido del XML
                    // de lo contrario insertara en bitacora la respuesta y devolvera la lista de valores en null
                    if (lstProductos[0].resultado.HRESULT ==1)
                    {
                        // Inserta en Bitacora la accion exitosa
                        Bitacora objBitacora = new Bitacora();
                        Bitacora.BitacoraModelo datBitacora = new Bitacora.BitacoraModelo();

                        datBitacora.Servicio = strServicio;
                        datBitacora.Operacion = strOperacion;
                        datBitacora.Instalacion = intInstalacion;
                        datBitacora.Usuario = strUsuario;
                        datBitacora.Producto = null;
                        datBitacora.Descripcion = "Consulta exitosa de Categorias / Productos";
                        datBitacora.CodRetornoServicio = lstProductos[0].resultado.HRESULT.ToString();
                        datBitacora.DescripcionRetornoServicio = lstProductos[0].resultado.mensaje.ToString();
                        datBitacora.OrigenError = "CRM";
                        datBitacora.XMLSolicitud = objWs.RequestXml;
                        datBitacora.XMLRespuesta = objWs.ResultString;
                        datBitacora.NodoWeb = null;
                        datBitacora.RegistraError = 0;

                        objBitacora.InsertarBitacora(datBitacora);

                        // Obtiene las categorias procesadas
                        CategoriasProducto lstCategoriaGestion = new CategoriasProducto();
                        lstCategoriaGestion = ObtenerCategorias(lstProductos, strPathImagenes);

                        return lstCategoriaGestion;
                    }
                    else
                    {
                        // Inserta en Bitacora si el HRESULT <> 1
                        Bitacora objBitacora = new Bitacora();
                        Bitacora.BitacoraModelo datBitacora = new Bitacora.BitacoraModelo();

                        datBitacora.Servicio = strServicio;
                        datBitacora.Operacion = strOperacion;
                        datBitacora.Instalacion = intInstalacion;
                        datBitacora.Usuario = strUsuario;
                        datBitacora.Producto = null;
                        datBitacora.Descripcion = "Consulta no exitosa de Categorias / Productos";
                        datBitacora.CodRetornoServicio = lstProductos[0].resultado.HRESULT.ToString();
                        datBitacora.DescripcionRetornoServicio = lstProductos[0].resultado.mensaje.ToString();
                        datBitacora.OrigenError = "CRM";
                        datBitacora.XMLSolicitud = objWs.RequestXml;
                        datBitacora.XMLRespuesta = objWs.ResultString;
                        datBitacora.NodoWeb = null;
                        datBitacora.RegistraError = 1;

                        objBitacora.InsertarBitacora(datBitacora);

                        // Agrega el retorno del mensaje de error
                        Mensaje msjError = new Mensaje()
                        {
                            Codigo = 3,
                            Nombre = "Error al obtener los datos de CRM, HRESULT <> 1",
                            ErrorTecnico = lstProductos[0].resultado.mensaje.ToString()
                        };

                        CategoriasProducto lstCategoriaGestion = new CategoriasProducto
                        {
                            Mensaje = msjError,
                            ProductoImagen = null
                        };

                        return lstCategoriaGestion;
                    }
                }
                catch (Exception e)
                {
                    // Si cambia alguna etiqueta en el mensaje original inserta en Bitacora el error
                    Bitacora objBitacora = new Bitacora();
                    Bitacora.BitacoraModelo datBitacora = new Bitacora.BitacoraModelo();

                    datBitacora.Servicio = strServicio;
                    datBitacora.Operacion = strOperacion;
                    datBitacora.Instalacion = intInstalacion;
                    datBitacora.Usuario = strUsuario;
                    datBitacora.Producto = null;
                    datBitacora.Descripcion = "Error en la estructura del XML " + e.ToString();
                    datBitacora.CodRetornoServicio = null;
                    datBitacora.DescripcionRetornoServicio = objWs.ResultString;
                    datBitacora.OrigenError = "CRM";
                    datBitacora.XMLSolicitud = objWs.RequestXml;
                    datBitacora.XMLRespuesta = objWs.ResultString;
                    datBitacora.NodoWeb = null;
                    datBitacora.RegistraError = 1;

                    objBitacora.InsertarBitacora(datBitacora);

                    // Agrega el retorno del mensaje de error
                    Mensaje msjError = new Mensaje()
                    {
                        Codigo = 4,
                        Nombre = "Error al obtener las etiquetas de CRM",
                        ErrorTecnico = objWs.ResultString
                    };

                    CategoriasProducto lstCategoriaGestion = new CategoriasProducto
                    {
                        Mensaje = msjError,
                        ProductoImagen = null
                    };

                    return lstCategoriaGestion;
                }
            }
            else
            {
                // Inserta en Bitacora el error
                Bitacora objBitacora = new Bitacora();
                Bitacora.BitacoraModelo datBitacora = new Bitacora.BitacoraModelo();

                datBitacora.Servicio = strServicio;
                datBitacora.Operacion = strOperacion;
                datBitacora.Instalacion = intInstalacion;
                datBitacora.Usuario = strUsuario;
                datBitacora.Producto = null;
                datBitacora.Descripcion = "Error al consumir el WS " + strUrl;
                datBitacora.CodRetornoServicio = null;
                datBitacora.DescripcionRetornoServicio = objWs.ResultString;
                datBitacora.OrigenError = "CRM";
                datBitacora.XMLSolicitud = objWs.RequestXml;
                datBitacora.XMLRespuesta = objWs.ResultString;
                datBitacora.NodoWeb = null;
                datBitacora.RegistraError = 1;

                objBitacora.InsertarBitacora(datBitacora);

                // Agrega el retorno del mensaje de error
                Mensaje msjError = new Mensaje()
                {
                    Codigo = 5,
                    Nombre = "Error al consumir el Web Service de CRM",
                    ErrorTecnico = objWs.ResultString
                };

                CategoriasProducto lstCategoriaGestion = new CategoriasProducto
                {
                    Mensaje = msjError,
                    ProductoImagen = null
                };

                return lstCategoriaGestion;
            }
        }
