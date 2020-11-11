using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Points.Application;
using Points.Application.Exceptions;
using Points.Models;

namespace Points.Controllers
{
    [ApiController]
    [Route("points")]
    public class PointsController : ControllerBase
    {
        private IPointsService Service { get; set; }

        public PointsController(IPointsService service)
        {
            Service = service;
        }

        [HttpGet("{userId}")]
        public ActionResult<IEnumerable<PointsSummary>> Get(string userId)
        {
            try
            {
                var summaries = Service.GetPointsSummaries(userId);

                return Ok(summaries);
            } catch(Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("{userId}")]
        public ActionResult Post(string userId, PointsTransaction transaction)
        {
            try
            {
                Service.AddPoints(userId, transaction);

                return Created("points", userId);
            } catch(InsufficientBalanceException ex)
            {
                return BadRequest(ex.Message);
            } catch(Exception)
            {
                // Normally you'd log this. You may or may not want to return some type of message
                return StatusCode(500);
            }
        }

        [HttpDelete("{userId}/{amount}")]
        public ActionResult Delete(string userId, int amount)
        {
            try {
                var transactions = Service.DeletePoints(userId, amount);
                return Ok(transactions);
            }
            catch (InsufficientBalanceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                // Normally you'd log this. You may or may not want to return some type of message
                return StatusCode(500);
            }
        }
    }
}
