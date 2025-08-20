using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.DTOs.TrainerDtos.RequestTrainer
{
    public class CatchPokemonRequest
    {
        public string Name { get; set; }
    }

    //una idea que me surge es que como en catch y
    //en battle pido el nombre, utilizar un solo request para los dos

}
