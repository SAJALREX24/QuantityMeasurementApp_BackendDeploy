using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppBusinessLayer.Interface;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace QuantityMeasurementApp.Api.Controller
{
    [Route("api/quantity")]
    [ApiController]
    public class QuantityMeasurementAPIController : ControllerBase
    {
        private readonly IQuantityMeasurementService Service;

        public QuantityMeasurementAPIController(IQuantityMeasurementService service)
        {
            Service = service;
        }

        // Guest-safe: userId = 0 if not logged in
        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : 0;
        }

        // POST api/quantity/compare  â€” no auth required
        [AllowAnonymous]
        [HttpPost("compare")]
        public IActionResult Compare([FromBody] QuantityInputDTO input)
        {
            if (input.QuantityOne == null || input.QuantityTwo == null ||
                string.IsNullOrWhiteSpace(input.QuantityOne.Unit) ||
                string.IsNullOrWhiteSpace(input.QuantityTwo.Unit))
                return BadRequest(new { message = "Invalid input." });

            var result = Service.Compare(input.QuantityOne, input.QuantityTwo, GetUserId());
            return Ok(result);
        }

        // POST api/quantity/add  â€” no auth required
        [AllowAnonymous]
        [HttpPost("add")]
        public IActionResult Add([FromBody] QuantityInputDTO input)
        {
            if (input.QuantityOne == null || input.QuantityTwo == null ||
                string.IsNullOrWhiteSpace(input.QuantityOne.Unit) ||
                string.IsNullOrWhiteSpace(input.QuantityTwo.Unit))
                return BadRequest(new { message = "Invalid input." });

            var result = Service.Add(input.QuantityOne, input.QuantityTwo, GetUserId());
            return Ok(result);
        }

        // POST api/quantity/subtract  â€” no auth required
        [AllowAnonymous]
        [HttpPost("subtract")]
        public IActionResult Subtract([FromBody] QuantityInputDTO input)
        {
            if (input.QuantityOne == null || input.QuantityTwo == null ||
                string.IsNullOrWhiteSpace(input.QuantityOne.Unit) ||
                string.IsNullOrWhiteSpace(input.QuantityTwo.Unit))
                return BadRequest(new { message = "Invalid input." });

            var result = Service.Subtract(input.QuantityOne, input.QuantityTwo, GetUserId());
            return Ok(result);
        }

        // POST api/quantity/divide  â€” no auth required
        [AllowAnonymous]
        [HttpPost("divide")]
        public IActionResult Divide([FromBody] QuantityInputDTO input)
        {
            if (input.QuantityOne == null || input.QuantityTwo == null ||
                string.IsNullOrWhiteSpace(input.QuantityOne.Unit) ||
                string.IsNullOrWhiteSpace(input.QuantityTwo.Unit))
                return BadRequest(new { message = "Invalid input." });

            var result = Service.Divide(input.QuantityOne, input.QuantityTwo, GetUserId());
            return Ok(result);
        }

        // POST api/quantity/convert  â€” no auth required
        [AllowAnonymous]
        [HttpPost("convert")]
        public IActionResult Convert([FromBody] ConvertDTO input)
        {
            if (input.QuantityOne == null || string.IsNullOrWhiteSpace(input.QuantityOne.Unit))
                return BadRequest(new { message = "Invalid input." });
            if (string.IsNullOrWhiteSpace(input.TargetUnit))
                return BadRequest(new { message = "Target unit is required." });

            var result = Service.Convert(input.QuantityOne, input.TargetUnit, GetUserId());
            return Ok(result);
        }

        // GET api/quantity/history  â€” AUTH REQUIRED
        [Authorize]
        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var history = Service.GetHistory(GetUserId());
            return Ok(history);
        }

        // DELETE api/quantity/history  â€” AUTH REQUIRED
        [Authorize]
        [HttpDelete("history")]
        public IActionResult DeleteHistory()
        {
            Service.DeleteHistory(GetUserId());
            return Ok(new { message = "History deleted successfully" });
        }

        // GET api/quantity/history/operation/{operationType}  â€” AUTH REQUIRED
        [Authorize]
        [HttpGet("history/operation/{operationType}")]
        public IActionResult GetHistoryByOperation(string operationType)
        {
            var history = Service.GetHistoryByOperation(GetUserId(), operationType);
            return Ok(history);
        }

        // GET api/quantity/history/type/{measurementType}  â€” AUTH REQUIRED
        [Authorize]
        [HttpGet("history/type/{measurementType}")]
        public IActionResult GetHistoryByType(string measurementType)
        {
            var history = Service.GetHistoryByType(GetUserId(), measurementType);
            return Ok(history);
        }

        // GET api/quantity/stats  â€” AUTH REQUIRED
        [Authorize]
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = Service.GetStats(GetUserId());
            return Ok(stats);
        }
    }
}
