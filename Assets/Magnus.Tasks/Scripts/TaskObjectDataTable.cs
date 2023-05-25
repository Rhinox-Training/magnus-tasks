
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.Vortex;
using Rhinox.Vortex.File;

namespace Rhinox.VOLT.Data.File
{
    [DataEndPoint(typeof(FileEndPoint))]
    public class TaskObjectDataTable : JsonFileDT<TaskObject>
    {
        protected override string _tableName => "task-objects";
        protected override int GetID(TaskObject dataObject)
        {
            return dataObject.ID;
        }

        protected override TaskObject SetID(TaskObject dto, int id)
        {
            dto.ID = id;
            return dto;
        }

        protected override ICollection<TaskObject> LoadData(bool createIfNotExists = false)
        {
            SceneHierarchyTree.Freeze();
            var result = base.LoadData(createIfNotExists);
            SceneHierarchyTree.UnFreeze();
            return result;
        }
    }
}