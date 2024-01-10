
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.Vortex;
using Rhinox.Vortex.File;

namespace Rhinox.Magnus.Tasks
{
    public class TaskObjectDataTable : DataTable<TaskObject>
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

        protected override TaskObject[] HandleLoadData(bool createIfNotExists = false)
        {
            SceneHierarchyTree.Freeze();
            var result = base.HandleLoadData(createIfNotExists);
            SceneHierarchyTree.UnFreeze();
            return result;
        }
    }
}