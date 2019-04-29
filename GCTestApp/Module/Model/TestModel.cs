using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using FluentValidation;

namespace GCTestApp.Module.Model
{
    public class TestModel : INotifyDataErrorInfo
    {
        TestValidator _validator  ;
        public TestModel()
        {
            _validator = new TestValidator();

        }

        public string Name { get; set; }
        public string Description{ get; set; }


        public IEnumerable GetErrors(string propertyName)
        {
            return _validator.Validate(this).Errors;
        }

        public bool HasErrors
        {
            get
            {
                return !_validator.Validate(this).IsValid;
            }
        }

        public void RaiseErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    }
}