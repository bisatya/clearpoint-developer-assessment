import './App.css'
import { Image, Button, Container, Row, Col, Form, Table, Stack } from 'react-bootstrap'
import React, { useState, useEffect } from 'react'
import Instructions from './components/Instructions'
import NewTodoItem from './components/NewTodoItem'
import TodoItemList from './components/TodoItemList'

const App = () => {
  /*
    Note:
    The idea is to update this "refetchToken" everytime an operation that changes the DB occurs (e.g., add new todo item / mark as complete)
    When the refetchToken is updated, it will trigger the useEffect method inside TodoItemList to re-fetch the data from the server
    This way, the list of todo items can be stored inside the TodoItemList component
    Therefore, the App.js component is free from any fetching responsibilities
  */  
  const [refetchToken, setRefetchToken] = useState(null);

  const onItemAdded = (id) => setRefetchToken(new Date().getTime());
  const onItemUpdated = (id) => setRefetchToken(new Date().getTime());

  return (
    <div className="App">
      <Container>
        <Row>
          <Col>
            <Image src="clearPointLogo.png" fluid rounded />
          </Col>
        </Row>
        <Row>
          <Col>
            <Instructions />
          </Col>
        </Row>
        <Row>
          <Col>
            <NewTodoItem onItemAdded={onItemAdded} />
          </Col>
        </Row>
        <br />
        <Row>
          <Col>
            <TodoItemList refetchToken={refetchToken} onItemUpdated={onItemUpdated} />
          </Col>
        </Row>
      </Container>
      <footer className="page-footer font-small teal pt-4">
        <div className="footer-copyright text-center py-3">
          © 2021 Copyright:
          <a href="https://clearpoint.digital" target="_blank" rel="noreferrer">
            clearpoint.digital
          </a>
        </div>
      </footer>
    </div>
  )
}

export default App
