using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using GCTestApp.Module.Model;

namespace GCTestApp.Module
{
   
    /// <summary>
    /// Бизнес правила проверки для нового документа.
    /// </summary>
    internal class TestValidator : AbstractValidator<TestModel>
    {
        /// <summary>
        /// Конструктор бизнес правил проверки для нового документа, по умолчанию.
        /// </summary>
        public TestValidator()
        {
            RuleFor(x => x.Description).Must(x=>x.Length > 1).WithMessage("Количество символов больше 1");
        }
    }
}
