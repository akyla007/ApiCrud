using ApiCrud.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes
{
    public static class EstudantesRotas
    {
        public static void AddRotasEstudantes(this WebApplication app)
        {
            var rotasEstudantes = app.MapGroup(prefix: "Estudantes");

            rotasEstudantes.MapPost("", handler: async (AddEstudantesRequest request, AppDbContext context, CancellationToken ct) =>
            {
                
                var alreadyExists = await context.Estudantes.AnyAsync(estudante => estudante.Nome == request.Nome);

                if (alreadyExists)
                {
                    return Results.BadRequest("Estudante já cadastrado");
                }

                var novoEstudante = new Estudante(request.Nome);

                await context.Estudantes.AddAsync(novoEstudante, ct);
                await context.SaveChangesAsync(ct);

                var filter = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);

                return Results.Ok(filter);
            });

            rotasEstudantes.MapGet("", handler: async (AppDbContext context, CancellationToken ct) =>
            {
                var estudantes = await context.Estudantes
                .Where(estudante => estudante.Ativo)
                .Select(estudante => new EstudanteDto(estudante.Id, estudante.Nome))
                .ToListAsync(ct);

                return estudantes;
            });

            rotasEstudantes.MapPut("{id}", async (Guid id, UpdateEstudanteRequest req, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes
                .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if (estudante is null)
                {
                    return Results.NotFound();
                }

                estudante.AtualizarNome(req.Nome);

                await context.SaveChangesAsync(ct);

                return Results.Ok(new EstudanteDto(estudante.Id, estudante.Nome));
            });

            rotasEstudantes.MapDelete("{id}", async (Guid id, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes
                .SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                if (estudante is null)
                {
                    return Results.NotFound();
                }

                estudante.Desativar();
                await context.SaveChangesAsync(ct);
                return Results.Ok();

            });
        }
    }
}
