using System;
using GCTestApp.Module.Model;

namespace GCTestApp.Module.ViewModel
{
    public class TestViewModel
    {
        public TestViewModel()
        {
            Model = new TestModel();
            Model.Name = "edfsdf";
            Model.Description = DateTime.Now.ToString();
        }

        public TestModel Model { get; set; }
        
    }
}