using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RuletaApi.Context;
using RuletaApi.Models;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RuletaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly AppDBContext _context;

        public GameController(AppDBContext context)
        {
            _context = context;
        }

        [HttpPost("/RuletaNueva")]
        public async Task<ActionResult<Ruleta>> CreaRuleta()
        {
            string estado_inicial = "creada";
            var rul = new Ruleta();
            rul.estado = estado_inicial;
            _context.Ruletas.Add(rul);
            await _context.SaveChangesAsync();
            return Ok("Id Ruleta: " + rul.id);
        }
        [HttpPost("/UsuarioNuevo")]
        public async Task<ActionResult<Ruleta>> CreaUsuario(int saldo)
        {
            var user = new Usuario();
            user.dineroDisponible = saldo;
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();
            return Ok("Id Usuario: " + user.id + ", Saldo: " + user.dineroDisponible);
        }

        [HttpPut("/OpenRul{id}")]
        public async Task<IActionResult> AbrirRuleta(int id)
        {
            var rul = await _context.Ruletas.FirstOrDefaultAsync(rulId => rulId.id == id);
            if (rul == null)
            {
                return BadRequest("Operación Denegada: Ruleta no existe");
            }
            else if (rul.estado == "abierta")
            {
                return BadRequest("Operación Denegada: Ruleta ya abierta");
            }
            else if (rul.estado == "cerrada")
            {
                return BadRequest("Operación Denegada: Ruleta cerrada");
            }
            else
            {
                rul.estado = "abierta";
                await _context.SaveChangesAsync();
                return Ok("Operación Exitosa");
            }
        }
        [HttpPost("/ApuestaNueva")]
        public async Task<ActionResult<Apuesta>> CrearApuesta(int IdRuleta, int IdUsuario, string tipo_apuesta, string apuesta, int monto_apuesta)
        {
            var rul = await _context.Ruletas.FirstOrDefaultAsync(rulId => rulId.id == IdRuleta);
            var user = await _context.Usuarios.FirstOrDefaultAsync(userId => userId.id == IdUsuario);

            if (rul == null)
            {
                return BadRequest("Operación Denegada: Ruleta no existe");
            }
            else if (rul.estado != "abierta")
            {
                return BadRequest("Operración Denegada: Ruleta no abierta o no disponible");
            }

            if (user == null)
            {
                return BadRequest("Operación Denegada: Usuario no existe");
            }
            else if (user.dineroDisponible < monto_apuesta)
            {
                return BadRequest("Operación Denegada: Fondos insuficientes");
            }
            else if (monto_apuesta > 10000) 
            {
                return BadRequest("Operación Denegada: Excede el monto máximo");
            }

            var nueva_apuesta = new Apuesta();
            nueva_apuesta.idRuleta = IdRuleta;
            nueva_apuesta.idUsuario = IdUsuario;

            if (tipo_apuesta == "numero")
            {
                if(Int32.Parse(apuesta)>=0 && Int32.Parse(apuesta) <= 36)
                {
                    nueva_apuesta.tipoApuesta = tipo_apuesta;
                    nueva_apuesta.apuestaNumero = Int32.Parse(apuesta);
                    nueva_apuesta.valor = monto_apuesta;
                }
                else
                {
                    return BadRequest("Operación Denegada: Apuesta no válida");
                }
            }
            else if (tipo_apuesta == "color") 
            {
                if(apuesta == "negro")
                {
                    nueva_apuesta.tipoApuesta = tipo_apuesta;
                    nueva_apuesta.apuestaColor = true;
                    nueva_apuesta.valor = monto_apuesta;
                }
                else if (apuesta == "rojo")
                {
                    nueva_apuesta.tipoApuesta = tipo_apuesta;
                    nueva_apuesta.apuestaColor = false;
                    nueva_apuesta.valor = monto_apuesta;
                }
                else
                {
                    return BadRequest("Operación Denegada: Apuesta no válida");
                }
            }
            else
            {
                return BadRequest("Operación Denegada: Tipo de apuesta no válido");
            }
            _context.Apuestas.Add(nueva_apuesta);
            user.dineroDisponible = user.dineroDisponible - monto_apuesta;
            await _context.SaveChangesAsync();
            return Ok("Apuesta Realizada");
        }
        [HttpPut("/CerrarRuleta{id}")]
        public async Task<IActionResult> CerrarRuleta(int id)
        {
            var rul = await _context.Ruletas.FirstOrDefaultAsync(rulId => rulId.id == id);
            if (rul == null)
            {
                return BadRequest("Operación Denegada: Ruleta no existe");
            }
            else if(rul.estado != "abierta")
            {
                return BadRequest("Operación Denedaga: Estado de ruleta inválido");
            }

            Random random = new Random();
            int num = random.Next(0, 36);
            bool color = false;
            if (num % 2 != 0) {
                color = true;
            }

            List<Apuesta> apuestas_ruleta = await _context.Apuestas.Where(rulId => rulId.idRuleta == id).ToListAsync();

            var ganadores = new Dictionary<int, int>();
            if (apuestas_ruleta == null)
            {
                return BadRequest("Operación denegada: La ruleta no tiene apuestas");
            }
            else
            {
                foreach (var apuesta in apuestas_ruleta)
                {
                    bool gana = false;
                    int ganancia = 0;

                    if(apuesta.tipoApuesta == "numero")
                    {
                        if(apuesta.apuestaNumero == num)
                        {
                            gana = true;
                            ganancia = apuesta.valor * 5;
                        }
                    }
                    else if(apuesta.tipoApuesta == "color")
                    {
                        if(apuesta.apuestaColor == color)
                        {
                            gana = true;
                            ganancia = (int)Math.Round(apuesta.valor * 1.8, MidpointRounding.AwayFromZero);
                        }
                    }

                    if (gana)
                    {
                        if (ganadores.ContainsKey(apuesta.idUsuario))
                        {
                            ganadores[apuesta.idUsuario] += ganancia;
                        }
                        else
                        {
                            ganadores[apuesta.idUsuario] = ganancia;
                        }
                    }

                }
                rul.estado = "cerrada";
                await _context.SaveChangesAsync();
                if (ganadores == null)
                {
                    return Ok("Ruleta Cerrada, no hay ganadores");
                }
                else
                {
                    foreach(var ganador in ganadores)
                    {
                        var user = await _context.Usuarios.FirstOrDefaultAsync(userId => userId.id == ganador.Key);
                        user.dineroDisponible = user.dineroDisponible + ganador.Value;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return Ok("Ruleta Cerrada, ganancias actualizadas");            
        }
    }
}
