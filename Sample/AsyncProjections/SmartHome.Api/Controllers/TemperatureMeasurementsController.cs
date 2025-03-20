using Core.Commands;
using Core.Ids;
using Core.Queries;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Temperature.TemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;
using SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

namespace SmartHome.Api.Controllers;

[Route("api/temperature-measurements")]
public class TemperatureMeasurementsController(
    ICommandBus commandBus,
    IQueryBus queryBus,
    IIdGenerator idGenerator)
    : Controller
{
    [HttpGet]
    public Task<IReadOnlyList<TemperatureMeasurement>> Get() =>
        queryBus.QueryAsync<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>(
            new GetTemperatureMeasurements()
        );

    [HttpPost]
    public async Task<IActionResult> Start()
    {
        var measurementId = idGenerator.New();

        var command = StartTemperatureMeasurement.Create(
            measurementId
        );

        await commandBus.SendAsync(command);

        return Created($"/api/TemperatureMeasurements/{measurementId}", measurementId);
    }


    [HttpPost("{id}/temperatures")]
    public async Task<IActionResult> Record(Guid id, [FromBody] decimal temperature)
    {
        var command = RecordTemperature.Create(
            id,
            temperature
        );

        await commandBus.SendAsync(command);

        return Ok();
    }
}
