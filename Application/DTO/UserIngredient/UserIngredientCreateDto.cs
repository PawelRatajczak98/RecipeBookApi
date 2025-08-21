namespace Application.DTO.UserIngredient
{
    public class UserIngredientCreateDto
    {
        public int IngredientId {  get; set; }
        public string IngredientName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }
}
