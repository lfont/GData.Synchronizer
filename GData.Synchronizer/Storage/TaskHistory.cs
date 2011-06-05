using System;
using PetaPoco;

namespace GData.Synchronizer.Storage
{
	[TableName("TaskHistory")]
	[PrimaryKey("Id")]
    public class TaskHistory
    {
        public int Id { get; set; }
		public string TaskId { get; set; }
        public DateTime Updated { get; set; }
        public string Uri { get; set; }
    }
}
