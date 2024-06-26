﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient; //lo importamos para establecer una conexion a bases de datos
using dominio; //va a usar clases de la libreria de clases dominio (alli reciden todos los modelos de dominio)


namespace negocio
{
    public class PokemonNegocio  //aca creamos los metodos de accesos a datos
    {
        /*Creamos un metodo public para que pueda ser accedido desde el exterior. Para que lea registros de la DB creamos
        una lista. 
         */
        public List<Pokemon> listar() 
        {
            List<Pokemon> lista = new List<Pokemon>();

            SqlConnection conexion = new SqlConnection(); //Nos permite extrablecer una conexion a la DB
            SqlCommand comando = new SqlCommand(); //Nos permite realizar acciones en la DB
            SqlDataReader lector; //Nos permite obtener en un vector un set de datos de la DB. No hace falta el uso del constructor.

            try //para poder manejar excepciones. En try toda la funcionalidad que puede fallar. 
            {
                //- - - CONFIGURACIONES - - - 
                //conexion.ConnectionString = "server=DESKTOP-V8QQ7P3\\SQLEXPRESS"; //forma 1 de la Cadena de Conexion
                conexion.ConnectionString = "server=.\\SQLEXPRESS; database=POKEDEX_DB; integrated security=true"; //forma 2  de la Cadena de Conexion
                                   /*a donde nos vamos a conectar; a que base de datos; "como" me voy a conectar */
                comando.CommandType = System.Data.CommandType.Text; /*hay tres tipos de comandos(realizar la accion):
                                                                     tipo texto, procedimiento almacenado, enlace directo 
                                                                     con la tabla*/
                //CONSULTA SOLO CON 1 TABLA
                //comando.CommandText = "Select Numero, Nombre, Descripcion, UrlImagen from POKEMONS"; //consulta que enviamos a la DB

                //CONSULTA CON 2 TABLAS
                comando.CommandText = "Select Numero, Nombre, P.Descripcion, UrlImagen, E.Descripcion As Tipo, D.Descripcion As Debilidad, P.IdTipo, P.IdDebilidad, P.Id From POKEMONS P, ELEMENTOS E, ELEMENTOS D Where E.Id = P.IdTipo and D.Id = P.IdDebilidad and P.Activo = 1"; 

                comando.Connection = conexion; //va a ejecutar el comando de la linea anterior

                conexion.Open(); // abrir la conexion
                lector = comando.ExecuteReader(); //realizo la lectura

                while (lector.Read()) //si hay un registro a continuacion devuelve true, y apuntando a c/u los registros de la DB
                {
                    Pokemon aux = new Pokemon();
                    aux.Id = (int)lector["Id"];
                    /**/
                    //aux.Numero = lector.GetInt32(0); //1ra forma

                    /* (casteo explicito segun el tipo de dato en la DB) el lector de la DB ["Columna virtual tal cual aparece en commandText"]*/
                    aux.Numero = (int)lector["Numero"]; //2da forma (quiza es mas practico)

                    aux.Nombre = (string)lector["Nombre"];
                    aux.Descripcion = (string)lector["Descripcion"];

                    //VALIDAR NULLS
                    //1RA FORMA
                    //if (!(lector.IsDBNull(lector.GetOrdinal("UrlImagen")))) // Si NO es nulo en la Columna UrlImagen, lo va a leer
                    //{                                                       //GetOrdinal es para obtener la columna
                    //    aux.UrlImagen = (string)lector["UrlImagen"];
                    //}
                    //2DA FORMA
                    if (!(lector["UrlImagen"] is DBNull)) 
                    {
                        aux.UrlImagen = (string)lector["UrlImagen"];
                    }

                    //como tipo no va a tener una instancia, porque cuando haga Tipo.Descripcion va a dar referencia nula
                    aux.Tipo = new Elemento(); //creo una instancia de la clase Elemento
                    aux.Tipo.Id = (int)lector["IdTipo"]; //con este dato el pokemon va a poder traer el id de su tipo...
                    aux.Tipo.Descripcion = (string)lector["Tipo"];
                    aux.Debilidad = new Elemento(); //idem que el anterior
                    aux.Debilidad.Id = (int)lector["IdDebilidad"]; //con este dato el pokemon va a poder traer el id de su debilidad...
                    aux.Debilidad.Descripcion = (string)lector["Debilidad"];

                    lista.Add(aux); //agrega dato a la lista
                }

                conexion.Close();
                return lista; //retorna la lista
            }

            catch (Exception ex) // retorna un error si algo sale mal.
            {

                throw ex;
            }

        }

        public void agregar(Pokemon nuevo) //agregar un nuevo poke a la db
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                //seteamos la consulta
                //1RA FORMA
                //datos.setearConsulta("Insert Into POKEMONS (Numero, Nombre, Descripcion, Activo, IdTipo, IdDebilidad) Values (" + nuevo.Numero + ",'" + nuevo.Nombre + "','"+ nuevo.Descripcion +"',1, "+ nuevo.Tipo.Id + ", "+ nuevo.Debilidad.Id +")"); //El activo esta por defecto en 1, pero tranquilamente podria estar por defecto en 1 PERO EN LA DB

                //2DA FORMA 
                datos.setearConsulta("Insert Into POKEMONS (Numero, Nombre, Descripcion, Activo, IdTipo, IdDebilidad, UrlImagen) Values (@Numero, @Nombre, @Descripcion, 1, @IdTipo, @IdDebilidad, @UrlImagen)"); //"@algo" especie de variables 
                //No podemos usar "comando.CommandText" ya que comando esta encapsulado.Para ello...
                datos.setearParametro("@Numero", nuevo.Numero); //y esto va a reemplazar a c/u de los parametros del value de la anterior consulta
                datos.setearParametro("@Nombre", nuevo.Nombre);
                datos.setearParametro("@Descripcion", nuevo.Descripcion);
                datos.setearParametro("@IdTipo", nuevo.Tipo.Id); 
                datos.setearParametro("@IdDebilidad", nuevo.Debilidad.Id);
                datos.setearParametro("@UrlImagen", nuevo.UrlImagen);

                //datos.ejecutarLectura; Esto no se debe hacer, ya que esto no es una lectura, es un INSERT
                datos.ejecutarAccion();

            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void modificar(Pokemon poke)  //modifica un poke de la db
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("Update POKEMONS set Numero = @numero, Nombre = @nombre, Descripcion = @descripcion, UrlImagen = @urlImagen, IdTipo = @idTipo, IdDebilidad = @idDebilidad Where Id = @id");
                datos.setearParametro("@numero", poke.Numero);
                datos.setearParametro("@nombre", poke.Nombre);
                datos.setearParametro("@descripcion", poke.Descripcion);
                datos.setearParametro("@urlImagen", poke.UrlImagen);
                datos.setearParametro("@idTipo", poke.Tipo.Id);
                datos.setearParametro("@idDebilidad", poke.Debilidad.Id);
                datos.setearParametro("@id", poke.Id);

                datos.ejecutarLectura();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void eliminar(int id) //elimina un poke de la db
        {
            try
            {
                AccesoDatos datos = new AccesoDatos();
                datos.setearConsulta("Delete From POKEMONS Where Id = @id");
                datos.setearParametro("@id",id);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 

        public void eliminarLogico(int id) //cambia el estado de un poke a INACTIVO
        {
            try
            {
                AccesoDatos datos = new AccesoDatos();
                datos.setearConsulta("Update POKEMONS Set Activo=0 Where Id = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public List<Pokemon> filtrar(string campo, string criterio, string filtro) 
            //FILTRO AVANZADO, va a la DB
        {
            List <Pokemon> lista = new List<Pokemon>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string consulta = "Select Numero, Nombre, P.Descripcion, UrlImagen, E.Descripcion As Tipo, D.Descripcion As Debilidad, P.IdTipo, P.IdDebilidad, P.Id From POKEMONS P, ELEMENTOS E, ELEMENTOS D Where E.Id = P.IdTipo And D.Id = P.IdDebilidad And P.Activo = 1 And ";
                //dejamos espacion para concatenar posibles filtros...

                switch (campo)
                {
                    case "Número":
                        switch (criterio)
                        {
                            case "Mayor a":
                                consulta += "Numero > " + filtro;
                                break;

                            case "Menor a":
                                consulta += "Numero < " + filtro;
                                break;

                            case "Igual a":
                                consulta += "Numero = " + filtro;
                                break;

                            default:
                                break;
                        }                  
                        break;

                    case "Nombre":
                        switch (criterio) 
                        {
                            case "Comenza con":
                                consulta += "Nombre LIKE '"+ filtro +"%'";
                                break;

                            case "Termina con":
                                consulta += "Nombre LIKE '%"+ filtro +"'";
                                break;

                            case "Contiene":
                                consulta += "Nombre LIKE '%" + filtro +"%'";
                                break;

                            default:
                                break;
                        }
                        break;

                    case "Descripción":
                        switch (criterio)
                        {
                            case "Comenza con":
                                consulta += "P.Descripcion LIKE '" + filtro + "%'";
                                break;

                            case "Termina con":
                                consulta += "P.Descripcion LIKE '%" + filtro + "'";
                                break;

                            case "Contiene":
                                consulta += "P.Descripcion LIKE '%" + filtro + "%'";
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                datos.setearConsulta(consulta);
                datos.ejecutarLectura();

                while (datos.Lector.Read()) //esto esta repetido, podria mejorarse...
                {
                    Pokemon aux = new Pokemon();
                    aux.Id = (int)datos.Lector["Id"];
                    aux.Numero = (int)datos.Lector["Numero"];
                    aux.Nombre = (string)datos.Lector["Nombre"];
                    aux.Descripcion = (string)datos.Lector["Descripcion"];

                    if (!(datos.Lector["UrlImagen"] is DBNull))
                    {
                        aux.UrlImagen = (string)datos.Lector["UrlImagen"];
                    }

                    aux.Tipo = new Elemento(); //creo una instancia de la clase Elemento
                    aux.Tipo.Id = (int)datos.Lector["IdTipo"]; //con este dato el pokemon va a poder traer el id de su tipo...
                    aux.Tipo.Descripcion = (string)datos.Lector["Tipo"];
                    aux.Debilidad = new Elemento(); //idem que el anterior
                    aux.Debilidad.Id = (int)datos.Lector["IdDebilidad"]; //con este dato el pokemon va a poder traer el id de su debilidad...
                    aux.Debilidad.Descripcion = (string)datos.Lector["Debilidad"];

                    lista.Add(aux); //agrega dato a la lista
                }

                return lista; //retorna una nueva lista con el/los pokemons segun las especificaciones del filtro avanzado

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
