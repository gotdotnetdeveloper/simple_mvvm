using System.ComponentModel;
using System.Linq;
using FluentValidation;

namespace GCTestApp.Module.Model
{
    public class TestModel : IDataErrorInfo
    {
        TestValidator _validator  ;
        public TestModel()
        {
            _validator = new TestValidator();

        }

        public string Name { get; set; }
        public string Description{ get; set; }

        /// <summary>
        /// Индексатор, в котором указано конкретное свойство, в котором произошла ошибка.
        /// </summary>
        /// <param name="columnName">Наименование проверяемого свойства.</param>
        /// <returns>Текст ошибки или пустая строка если ошибки отсутствуют.</returns>
        public string this[string columnName] =>
            _validator.Validate(this, columnName).Errors.FirstOrDefault()?.ErrorMessage;

        /// <summary>
        /// Строка - указание общей ошибки.
        /// </summary>
        public string Error => _validator.Validate(this).Errors.FirstOrDefault()?.ErrorMessage;
    }
}