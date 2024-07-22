using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using StudentManagement.Entity;
using StudentManagement.Model;

using System.Data;

namespace StudentManagement.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class StudentController : Controller
    {
        public string URI = "https://localhost:8081";
        public string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        public string DatabaseName = "StudentDatabase";
        public string ContainerName = "Student";

        public Container container;
        public StudentController()
        {

            container = GetContainer();
        }
        private Container GetContainer()
        {
            CosmosClient cosmosClient = new CosmosClient(URI, PrimaryKey);
            Database database = cosmosClient.GetDatabase(DatabaseName);
            Container container = database.GetContainer(ContainerName);
            return container;
        }
        
        //to add the record in the database

        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentModel studentModel)
       {
         //  studentModel.Id = Guid.NewGuid().ToString();
           var student = await container.CreateItemAsync(studentModel); //CreateItemAsynce used to store the values in the database
           return Ok(student);

        }

        [HttpGet("{Id}")]
        public async Task<StudentModel> GetStudentByID(string Id)
        {
            var students = container.GetItemLinqQueryable<StudentModel>(true).Where(q => q.Id == Id).FirstOrDefault();
           return students;

        }
        [HttpGet]
        public async Task<List<StudentModel>> GetAllStudents()
        {
            //query to get all records
            var students =  container.GetItemLinqQueryable<StudentModel>(true).ToList();

          //return the result
          return students;

        }

        [HttpPost]
        public async Task<StudentModel> AddStudentEntity(StudentModel studentModel)
        {
            // 1.Created the object of entity and map all the fields from model to entity
            StudentEntity student = new StudentEntity();
            student.RollNo = studentModel.RollNo;
            student.StudentName = studentModel.StudentName;
            student.PhoneNumber = studentModel.PhoneNumber;
            student.Age = studentModel.Age;

            //2. Assign values to mandatary fields
            student.Id = Guid.NewGuid().ToString();
            student.Uid = student.Id;
            student.Documentype = "student";
            student.CreatedBy = "Sneha";
            student.CreatedOn = DateTime.Now;
            student.UpdatedBy = "Newton";
            student.UpdatedOn = DateTime.Now;
            student.Version = 1;
            student.Active = true;
            student.Archived = false;

            //3. add the data to database
            StudentEntity response = await container.CreateItemAsync(student);

            //4.return the model
            StudentModel responseModel = new StudentModel();
            responseModel.RollNo = response.RollNo;
            responseModel.StudentName = response.StudentName;
            responseModel.PhoneNumber = response.PhoneNumber;
            responseModel.Age = response.Age;
            return responseModel;
        }

        [HttpGet]
        public async Task<List<StudentModel>> GetAllStudent()
        {
            //1.fetch the all the records
            var students =  container.GetItemLinqQueryable<StudentEntity>(true)
                                    .Where(q => q.Active == true && q.Archived == false && q.Documentype == "student")
                                    .ToList();

            //map the field to the model
            List<StudentModel> studentModels = new List<StudentModel>();

            foreach (var student in students)
            {
                StudentModel model = new StudentModel();
                model.Uid = student.Uid;
                model.RollNo = student.RollNo;
                model.StudentName = student.StudentName;
                model.PhoneNumber = student.PhoneNumber;
                model.Age = student.Age;
                studentModels.Add(model);
            }

            return studentModels;
        }
        [HttpGet]
        public async Task <StudentModel> GetStudentByUid(string UId)
        {
          //get the record
           var student=container.GetItemLinqQueryable<StudentEntity>(true).Where(q => q.Uid == UId &&q.Active==true && q.Archived==false).FirstOrDefault();

            //map the fields
            if (student != null)
            {
                StudentModel studentmodel = new StudentModel();
                studentmodel.RollNo = student.RollNo;
                studentmodel.StudentName = student.StudentName;
                studentmodel.PhoneNumber = student.PhoneNumber;
                studentmodel.Age = student.Age;
                return studentmodel;
            }
            else
            {
                return null;
            }
            //we have taken the reference variable as StudentModel so we can return only student model 
            //if we return the student entity obj then it will throw error so thats why map the fields
            //return 
            
            
        }

        [HttpPost]

        public async Task<StudentModel> UpdateStudent(StudentModel student)
        {
            //1. get the existing record by uid
            var existingStudent = container.GetItemLinqQueryable<StudentEntity>(true).Where(q => q.Uid == student.Uid && q.Active == true && q.Archived == false).FirstOrDefault();

            //2. to replace the records
            existingStudent.Archived = true;
            existingStudent.Active = false;
            await container.ReplaceItemAsync(existingStudent, existingStudent.Id);


            //3. assign the values to manditory fields
            existingStudent.Id=Guid.NewGuid().ToString();
            existingStudent.UpdatedBy = "GondeDumala";
            existingStudent.UpdatedOn = DateTime.Now;
            existingStudent.Version=existingStudent.Version+1;
            existingStudent.Active = true;
            existingStudent.Archived = false;
            //4. assign the values to the field which we get from request object
            existingStudent.RollNo = student.RollNo;
            existingStudent.StudentName= student.StudentName;
            existingStudent.PhoneNumber = student.PhoneNumber;
            existingStudent.Age = student.Age;
            //5.to add the data to database
             existingStudent=await container.CreateItemAsync(existingStudent);



            //6.to return
            StudentModel response = new StudentModel();
            response.Uid= existingStudent.Uid;
            response.StudentName= existingStudent.StudentName;
            response.PhoneNumber = existingStudent.PhoneNumber;
            response.Age = existingStudent.Age;
            return response;

        }
        

    
    }
}
