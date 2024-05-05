const express = require('express');
const { Pool } = require('pg');
const app = express();

const pool = new Pool({
    host: 'postgres', // Nombre del servicio de contenedor de PostgreSQL en Docker Compose
    port: 5432,
    database: 'recomendaciones_libros',
    user: 'postgres',
    password: 'password'
});

// Función para verificar la conexión con PostgreSQL
pool.connect((err, client, release) => {
    if (err) {
        return console.error('Error al conectar a PostgreSQL:', err.stack);
    }
    console.log('Conexión exitosa a PostgreSQL.');
    release();
});

app.set('view engine', 'ejs');

app.get('/books', async (req, res) => {
    try {
        const result = await pool.query("SELECT * FROM books");
        const books = result.rows;
        res.render('index', { books });
        
    } catch(error) {
        console.error('Error al obtener los libros:', error);
        res.status(500).send('Error obteniendo los libros');
    }
});


// Ejemplo de consulta a la base de datos
pool.query('SELECT NOW()', (err, res) => {
    if (err) {
      console.error('Error al conectar con la base de datos', err);
    } else {
      console.log('Conexión exitosa a la base de datos');
      console.log('Resultado de la consulta:', res.rows[0]);
    }
  });
  

app.listen(3000, () => {
    console.log('Servidor Node.js escuchando en el puerto 3000');
});
