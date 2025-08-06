# New Update and UpdateAsync upgrade: CASE

## A new feature added to allow developers to programmatically set CASE WHEN when assigning values. Feature includes grouping in sub statements () or 
## to allow condition to point to a column variable instead of a direct paramater value. SQL injection friendly

## Original Update Statement for multiple records using anonymous objects:

###      foreach (var item in data)

###      {

###          object obj = new

###          {

###              MyField = item.Value

###          };

###          cnt += await QueryFactory.Query(tableName).Where("Id", item.Id).UpdateAsync(value);
       

###      }

###      return cnt;





## New Update with select case using multi-level array systems
## version 1 : allows is equal condition only for now
##             For the Else it will always fill with name of field itself , self assigning. 
##             This happens if format is wrong as well.
##             The else protects you fro your field to be set back to NULL
               
### Warning: Limitation is requires , Suggest 200 rows for low number columns, 
###          25 for higher number columns or clauses.


     var datac = data.Chunk(200); // breaking data up to 200 rows

     //each holds for each  update set, which allows multiple value setting as older Update
     List<object[]> cases = [];  

     if (datac.Any()) foreach (var d in datac)
     {
       
         try
         {                   
             foreach (var item in d)   //Build case when statement , standard 3
             {
                 cases.Add(["Id", item.Id, item.Value]); 
             }
             object obj = new
             {
                 MyField= cases.ToArray()
             };
             cases.Clear();

             //if data set is smaller than whole table , best to use in statement to reduce cost
             cnt += await QueryFactory.Query(tableName)
                    .WhereIn("Id", d.Select(dd => dd.Id).ToArray())
                    .UpdateAsync(value);             
         }
         catch { throw; }
         finally { cases.Clear();  }
     }
     else cases.Clear();

     return cnt;    




##standard: Case WHEN x = A then Y... END: 
### In your cases array the flow is [x,A,Y].
### Assignmet value is always last.





## Available Feaure 1 : While its common to do 3 items for basic, when can extend the criteria with AND and OR
## It combine, the array column after the orevioud criteria field must be an AND or OR, unless using , () or * explained later

### Note: Assignmet value is always last. you can use AND,&&,& or OR,||,|, <>. Not case sensitive.

### Case WHEN x = A AND z = B then Y  ... END: 
###      In your cases array the flow is [x,A,"AND",z,B,Y]    
### Case WHEN x = A OR z = B then Y  ... END: 
###      Array the flow is [x,A,"OR",z,B,Y]   



 
  
## Available Feaure 2 : Subset (). This allows seperating your "And" & "Or" blocks 
### ex: case when (a = 1 or a = 5) and (b = 7 and c = 2)
### This can be placed anywhere before the assignment column or * assignment column, 
### if you forget to add the ) to close, the engine
### will compensate.

### Case WHEN (x = A AND z = B) OR J = C then Y  ... END: 
###      Array the flow is ["(",x,A,"AND",z,B,")","OR",j,c,Y] 
### Case WHEN (x = A OR z = B) AND (J = C AND K = D) then Y  ... END: 
###      Array the flow is ["(",x,A,"OR",z,B,")","AND","(",j,c,"AND",k,d,")" Y]   


## Available Feaure 3 : To Another Column Field (*). This allows criteria to check if column equals another column (field)
### Case WHEN (colx = colb AND colz = colx) then Y  ... END: 
###      Array the flow is [,colx,*',colb,"AND",colz,colx, Y]   
