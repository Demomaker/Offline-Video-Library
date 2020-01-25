using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DVL
{
    class GeneralVideoMediaRepository : Repository<GeneralVideoMediaDTO>
    {
        private const string DATABASE_CONNECTION_STRING = "Data Source=Medias.sqlite;Version=3;";
        private const string DATABASE_CONNECTION_ERROR = "Database Connection could not be achieved";

        private const string ID_PARAMETER = "@id";
        private const string TITLE_PARAMETER = "@title";
        private const string DESCRIPTION_PARAMETER = "@description";
        private const string FILENAME_PARAMETER = "@filename";
        private const string FILELOCATION_PARAMETER = "@filelocation";
        private const string IMGPATH_PARAMETER = "@imgpath";

        private const string CREATE_TABLE_COMMAND = "CREATE TABLE IF NOT EXISTS medias (id_media INTEGER PRIMARY KEY, filename varchar(5000), img_path varchar(5000), title varchar(5000), description varchar(5000), filelocation varchar(5000));";
        private const string GET_WITH_ID_COMMAND = "SELECT id_media, title, description, filename, filelocation, img_path from medias WHERE id_media = " + ID_PARAMETER;
        private const string GET_ALL_COMMAND = "SELECT id_media, title, description, filename, filelocation, img_path from medias";
        private const string ADD_COMMAND = "INSERT INTO medias(id_media, title, description, filename, filelocation, img_path) VALUES(" + ID_PARAMETER + ", " + TITLE_PARAMETER + ", " + DESCRIPTION_PARAMETER + ", " + FILENAME_PARAMETER + ", " + FILELOCATION_PARAMETER + ", " + IMGPATH_PARAMETER + ")";
        private const string REMOVE_COMMAND = "DELETE FROM medias WHERE id_media = " + ID_PARAMETER;
        private const string UPDATE_COMMAND = "UPDATE medias SET title = " + TITLE_PARAMETER + ", description = " + DESCRIPTION_PARAMETER + ", filename = " + FILENAME_PARAMETER + ", filelocation = " + FILELOCATION_PARAMETER + ", img_path = " + IMGPATH_PARAMETER + " WHERE id_media = " + ID_PARAMETER;

        private SQLiteConnection m_dbConnection = null;

        public GeneralVideoMediaRepository()
        {
            m_dbConnection = new SQLiteConnection(DATABASE_CONNECTION_STRING);
        }

        public void CreateTable()
        {
            if (m_dbConnection == null) throw new Exception(DATABASE_CONNECTION_ERROR);
            m_dbConnection.Open();
            string sqlcreate = CREATE_TABLE_COMMAND;
            SQLiteCommand commandcreate = new SQLiteCommand(sqlcreate, m_dbConnection);
            commandcreate.ExecuteNonQuery();
        }

        public void Add(GeneralVideoMediaDTO addedObject, bool IncludeID = false)
        {
            GeneralVideoMediaDTO generalVideoMediaDTO = new GeneralVideoMediaDTO();
            SQLiteCommand command = new SQLiteCommand(ADD_COMMAND, m_dbConnection);
            if (IncludeID) command.Parameters.AddWithValue(ID_PARAMETER, addedObject.MediaId);
            if (!IncludeID) command.Parameters.AddWithValue(ID_PARAMETER, null);
            command.Parameters.AddWithValue(TITLE_PARAMETER, addedObject.Title);
            command.Parameters.AddWithValue(DESCRIPTION_PARAMETER, addedObject.Description);
            command.Parameters.AddWithValue(FILENAME_PARAMETER, addedObject.FileName);
            command.Parameters.AddWithValue(FILELOCATION_PARAMETER, addedObject.FileLocation);
            command.Parameters.AddWithValue(IMGPATH_PARAMETER, addedObject.ImgPath);
            try
            {
                command.ExecuteNonQuery();
            }

            catch (IOException e)
            {
                throw e;
            }
        }

        public List<GeneralVideoMediaDTO> GetAll()
        {
            List<GeneralVideoMediaDTO> generalVideoMediaDTOs = new List<GeneralVideoMediaDTO>();
            SQLiteCommand command = new SQLiteCommand(GET_ALL_COMMAND, m_dbConnection);
            try
            {
                using (var data = command.ExecuteReader())
                {
                    var id = 0;
                    var title = "";
                    var description = "";
                    var fileName = "";
                    var fileLocation = "";
                    var imgPath = "";
                    while (data.Read())
                    {
                        id = data.GetInt32(0);
                        title = data.GetString(1);
                        description = data.GetString(2);
                        fileName = data.GetString(3);
                        fileLocation = data.GetString(4);
                        imgPath = data.GetString(5);
                        generalVideoMediaDTOs.Add(new GeneralVideoMediaDTO(id, title, description, fileName, fileLocation, imgPath));
                    }
                    data.Close();
                }
            }

            catch (IOException e)
            {
                throw e;
            }
            return generalVideoMediaDTOs;
        }

        public List<GeneralVideoMediaDTO> GetAllWithCondition(Expression<Func<GeneralVideoMediaDTO, bool>> predicate)
        {
            return GetAll().AsQueryable().Where(predicate).ToList();
        }

        public GeneralVideoMediaDTO GetWithId(int Id)
        {
            GeneralVideoMediaDTO generalVideoMediaDTO = new GeneralVideoMediaDTO();
            SQLiteCommand command = new SQLiteCommand(GET_WITH_ID_COMMAND, m_dbConnection);
            command.Parameters.AddWithValue(ID_PARAMETER, Id);
            try
            {
                using (var data = command.ExecuteReader())
                {
                    var id = 0;
                    var title = "";
                    var description = "";
                    var fileName = "";
                    var fileLocation = "";
                    var imgPath = "";
                    while (data.Read())
                    {
                        id = data.GetInt32(0);
                        title = data.GetString(1);
                        description = data.GetString(2);
                        fileName = data.GetString(3);
                        fileLocation = data.GetString(4);
                        imgPath = data.GetString(5);
                    }

                    generalVideoMediaDTO = new GeneralVideoMediaDTO(id, title, description, fileName, fileLocation, imgPath);
                    data.Close();
                }
            }

            catch (IOException e)
            {
                throw e;
            }
            return generalVideoMediaDTO;
        }

        public void Remove(GeneralVideoMediaDTO removedObject)
        {
            RemoveWithId(removedObject.MediaId);
        }

        public void RemoveWithId(int Id)
        {
            GeneralVideoMediaDTO generalVideoMediaDTO = new GeneralVideoMediaDTO();
            SQLiteCommand command = new SQLiteCommand(REMOVE_COMMAND, m_dbConnection);
            command.Parameters.AddWithValue("@id", Id);
            try
            {
                command.ExecuteNonQuery();
            }

            catch (IOException e)
            {
                throw e;
            }
        }

        public void Update(GeneralVideoMediaDTO updatedObject)
        {
            GeneralVideoMediaDTO generalVideoMediaDTO = new GeneralVideoMediaDTO();
            SQLiteCommand command = new SQLiteCommand(UPDATE_COMMAND, m_dbConnection);
            command.Parameters.AddWithValue(ID_PARAMETER, updatedObject.MediaId);
            command.Parameters.AddWithValue(TITLE_PARAMETER, updatedObject.Title);
            command.Parameters.AddWithValue(DESCRIPTION_PARAMETER, updatedObject.Description);
            command.Parameters.AddWithValue(FILENAME_PARAMETER, updatedObject.FileName);
            command.Parameters.AddWithValue(FILELOCATION_PARAMETER, updatedObject.FileLocation);
            command.Parameters.AddWithValue(IMGPATH_PARAMETER, updatedObject.ImgPath);
            try
            {
                command.ExecuteNonQuery();
            }

            catch (IOException e)
            {
                throw e;
            }
        }
    }
}
