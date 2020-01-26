/* GeneralVideoMediaDTO.cs : DTO Representation of a GeneralVideoMedia
 * Author : Demomaker
 * Version : 1.0
 */

namespace DVL
{
    class GeneralVideoMediaDTO
    {
        private int mediaId;
        private string title;
        private string description;
        private string fileName;
        private string fileLocation;
        private string imgPath;

        public GeneralVideoMediaDTO() :
            this(0, "", "", "", "", "")
        {

        }

        public GeneralVideoMediaDTO(
            int mediaId = 0,
            string title = "",
            string description = "",
            string fileName = "",
            string fileLocation = "",
            string imgPath = ""
            )
        {
            MediaId = mediaId;
            Title = title;
            Description = description;
            FileName = fileName;
            FileLocation = fileLocation;
            ImgPath = imgPath;
        }

        public int MediaId
        {
            get
            {
                return mediaId;
            }

            private set
            {
                mediaId = value;
            }
        }
        public string Title
        {
            get
            {
                return title;
            }

            private set
            {
                title = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            private set
            {
                description = value;
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }

            private set
            {
                fileName = value;
            }
        }

        public string FileLocation
        {
            get
            {
                return fileLocation;
            }

            private set
            {
                fileLocation = value;
            }
        }

        public string ImgPath
        {
            get
            {
                return imgPath;
            }

            private set
            {
                imgPath = value;
            }
        }

        public void SetNewTitle(string newTitle)
        {
            Title = newTitle;
        }

        public void SetNewDescription(string newDescription)
        {
            Description = newDescription;
        }
    }
}
