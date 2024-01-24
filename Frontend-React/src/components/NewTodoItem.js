import axios from 'axios';
import React, { useState } from 'react';
import { Button, Container, Row, Col, Form, Stack } from 'react-bootstrap';

const NewTodoItem = (props) => {
    const [description, setDescription] = useState('');
    const [error, setError] = useState('');

    const handleDescriptionChange = (event) => {
        setDescription(event.target.value);
    }

    async function handleAdd() {
        try {
            const response = await axios.post('/api/todoitems', { description: description });
            setDescription('');
            setError('');
            props.onItemAdded && props.onItemAdded(response.data.id);
        } catch (error) {
            setError(error.response.data.errors && error.response.data.errors.Description[0] || error.response.data);
        }
    }
  
    function handleClear() {
        setDescription('');
        setError('');
    }

    return (
        <Container>
            <h1>Add Item</h1>
            <Form.Group as={Row} className="mb-3" controlId="formAddTodoItem">
                <Form.Label column sm="2">
                    Description
                </Form.Label>
                <Col md="6">
                    <Form.Control
                        type="text"
                        placeholder="Enter description..."
                        value={description}
                        onChange={handleDescriptionChange}
                        data-testid={'input-addtodoitem'}
                    />
                </Col>
            </Form.Group>
            <Form.Group as={Row} className="mb-3 offset-md-2" controlId="formAddTodoItem">
                <Stack direction="horizontal" gap={2}>
                    <Button variant="primary" onClick={handleAdd} data-testid={'button-addtodoitem'}>
                        Add Item
                    </Button>
                    <Button variant="secondary" onClick={handleClear}>
                        Clear
                    </Button>
                    {error && (<small className='text-danger'>{error}</small>)}
                </Stack>
            </Form.Group>
        </Container>
    );
}

export default NewTodoItem;