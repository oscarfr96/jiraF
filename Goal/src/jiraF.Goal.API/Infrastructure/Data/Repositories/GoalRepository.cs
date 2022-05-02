﻿using jiraF.Goal.API.Contracts;
using jiraF.Goal.API.Domain;
using jiraF.Goal.API.Domain.Dtos;
using jiraF.Goal.API.Infrastructure.Data.Contexts;
using jiraF.Goal.API.Infrastructure.Data.Entities;
using jiraF.Goal.API.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace jiraF.Goal.API.Infrastructure.Data.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly AppDbContext _dbContext;

    public GoalRepository(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<GoalModel>> GetAsync()
    {
        return await _dbContext.Goals
            .Select(x => new GoalModel(
                new Title(x.Title),
                new Description(x.Description),
                new User(),
                new User(),
                new LabelModel(new Title(x.LabelId.ToString()))))
            .ToListAsync();
    }

    public async Task<GoalModel> GetByIdAsync(Guid id)
    {
        return await _dbContext.Goals
            .Where(x => x.Id == id)
            .Select(x => new GoalModel(
                new Title(x.Title),
                new Description(x.Description),
                new User(),
                new User(),
                new LabelModel(new Title(x.LabelId.ToString()))))
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(GoalModel goal)
    {
        Guid labelId = await GetLabelIdByTitle(goal.Label.Title.Value);
        _dbContext.Goals.Add(new GoalEntity
        {
            Title = goal.Title.Value,
            AssigneeId = goal.Assignee.Number,
            ReporterId = goal.Reporter.Number,
            LabelId = labelId,
            Description = goal.Description.Value,
            DateOfCreate = goal.DateOfCreate,
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, GoalModel goal)
    {
        Guid labelId = await GetLabelIdByTitle(goal.Label.Title.Value);
        GoalEntity entity = await _dbContext.Goals.FirstOrDefaultAsync(x => x.Id == id);
        entity.Title = goal.Title.Value;
        entity.AssigneeId = goal.Assignee.Number;
        entity.ReporterId = goal.Reporter.Number;
        entity.DateOfCreate = goal.DateOfCreate;
        entity.DateOfUpdate = goal.DateOfUpdate;
        entity.Description = goal.Description.Value;
        entity.LabelId = labelId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        GoalEntity entity = await _dbContext.Goals.FirstOrDefaultAsync(x => x.Id == id);
        _dbContext.Goals.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }


    private async Task<Guid> GetLabelIdByTitle(string title)
    {
        return await _dbContext.Labels
            .Where(x => x.Title == title)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
    }
}
