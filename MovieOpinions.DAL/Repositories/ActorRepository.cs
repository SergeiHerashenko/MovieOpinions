using MovieOpinions.DAL.Connect_Database;
using MovieOpinions.DAL.Interface;
using MovieOpinions.Domain.Entity.Actors;
using MovieOpinions.Domain.Response;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieOpinions.DAL.Repositories
{
    public class ActorRepository : IActorRepository
    {
        public Task<BaseResponse<bool>> Create(Actor entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(Actor entity)
        {
            throw new NotImplementedException();
        }

        public async Task<Actor> GetActorName(string NameActor)
        {
            string[] parts = NameActor.Split(' ');
            Actor actor = new Actor();

            ConnectMovieOpinions connect = new ConnectMovieOpinions();

            using (var conn = new NpgsqlConnection(connect.ConnectMovieOpinionsDataBase()))
            {
                await conn.OpenAsync();
                try
                {
                    using (var GetActor = new NpgsqlCommand("SELECT id_actor, first_name_actor, last_name_actor FROM Actor_Table WHERE first_name_actor = @FirstName AND last_name_actor = @LastName", conn))
                    {
                        GetActor.Parameters.AddWithValue("FirstName", parts[0]);
                        GetActor.Parameters.AddWithValue("LastName", parts[1]);

                        using (var reader = await GetActor.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int actorId = Convert.ToInt32(reader["id_actor"]);

                                actor.FirstName = parts[0];
                                actor.LastName = parts[1];
                                actor.IdActor = actorId;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return actor;
        }

        public Task<Actor> Update(Actor entity)
        {
            throw new NotImplementedException();
        }
    }
}
